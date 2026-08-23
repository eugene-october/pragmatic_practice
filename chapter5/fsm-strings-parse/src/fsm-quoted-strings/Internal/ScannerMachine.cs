using Fsm.Kernel;

namespace Fsm.QuotedStrings.Internal;

/// <summary>
/// The scanner's grammar, written out as a table.
/// </summary>
/// <remarks>
/// <para>
/// This is the whole of the parsing logic. There is no other place where a decision about quotes or
/// escapes is made — the rest of the library only feeds characters in and reads segments out.
/// </para>
/// <para>
/// The table is built once into a single shared instance. It can be shared because it says nothing
/// about <em>which</em> characters quote or escape: that lives in the classifier and the context, so
/// every dialect runs the same table.
/// </para>
/// </remarks>
internal static class ScannerMachine
{
    internal static StateMachine<ScanState, CharClass, char, ScanContext> Instance { get; } = Build();

    private static StateMachine<ScanState, CharClass, char, ScanContext> Build()
        => StateMachine.For<ScanState, CharClass, char, ScanContext>()
            .Initial(ScanState.Outside)
            .Terminal(ScanState.Completed, ScanState.Failed)

            .From(ScanState.Outside)
                .On(CharClass.Quote).Do(ScanActions.BeginSegment).GoTo(ScanState.InQuotes)
                .On(CharClass.EndOfInput).GoTo(ScanState.Completed)
                // Prose between quoted runs, including a stray escape character, is not our business.
                .OnRemaining().Ignore().Stay()

            .From(ScanState.InQuotes)
                .On(CharClass.Quote).Do(ScanActions.CompleteSegment).GoTo(ScanState.Outside)
                .On(CharClass.Escape).Do(ScanActions.MarkEscape).GoTo(ScanState.AfterEscape)
                .On(CharClass.Literal).Do(ScanActions.Append).Stay()
                .On(CharClass.EndOfInput).Do(ScanActions.ReportUnterminated).GoTo(ScanState.Failed)

            .From(ScanState.AfterEscape)
                .On(CharClass.EndOfInput).Do(ScanActions.ReportDanglingEscape).GoTo(ScanState.Failed)
                // An escape suppresses meaning, so quote, escape and literal are all the same here;
                // whether the pair is legal is the dialect's business, asked once, as a guard.
                .OnRemaining()
                    .When(ScanGuards.CanDecode).Do(ScanActions.AppendDecoded).GoTo(ScanState.InQuotes)
                    .Otherwise().Do(ScanActions.ReportUnknownEscape).GoTo(ScanState.Failed)

            .Build();
}
