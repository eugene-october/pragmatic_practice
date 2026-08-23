namespace Fsm.QuotedStrings.Internal;

/// <summary>
/// The side effects the scanner's transitions perform.
/// </summary>
/// <remarks>
/// Static methods, so the delegates the table holds are cached by the runtime and no closure is
/// allocated. Their names are also what the generated diagram labels its edges with.
/// </remarks>
internal static class ScanActions
{
    internal static void BeginSegment(in char symbol, ScanContext context) => context.BeginSegment();

    internal static void MarkEscape(in char symbol, ScanContext context) => context.MarkEscape();

    internal static void Append(in char symbol, ScanContext context) => context.Append(symbol);

    /// <summary>
    /// Appends the character an escape sequence stands for. Only reached behind
    /// <see cref="ScanGuards.CanDecode"/>, so the decode cannot fail here.
    /// </summary>
    internal static void AppendDecoded(in char symbol, ScanContext context)
    {
        context.Syntax.EscapeDecoder.TryDecode(symbol, out var decoded);
        context.Append(decoded);
    }

    internal static void CompleteSegment(in char symbol, ScanContext context) => context.CompleteSegment();

    internal static void ReportUnterminated(in char symbol, ScanContext context)
        => context.Fail(
            ScanErrorKind.UnterminatedString,
            context.SegmentStart,
            $"The quoted string opened at {context.SegmentStart} is never closed.");

    internal static void ReportDanglingEscape(in char symbol, ScanContext context)
        => context.Fail(
            ScanErrorKind.DanglingEscape,
            context.EscapeStart,
            $"The input ends with the escape character '{context.Syntax.Escape}' at {context.EscapeStart}.");

    internal static void ReportUnknownEscape(in char symbol, ScanContext context)
        => context.Fail(
            ScanErrorKind.UnknownEscapeSequence,
            context.EscapeStart,
            $"'{context.Syntax.Escape}{symbol}' at {context.EscapeStart} is not a recognised escape sequence.");
}

/// <summary>
/// The conditions the scanner's transitions branch on.
/// </summary>
/// <remarks>
/// Guards must be free of side effects: the engine may ask several of them before one accepts.
/// </remarks>
internal static class ScanGuards
{
    /// <summary>True when the active dialect recognises the escape sequence being consumed.</summary>
    internal static bool CanDecode(in char symbol, ScanContext context)
        => context.Syntax.EscapeDecoder.TryDecode(symbol, out _);
}
