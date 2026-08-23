namespace Fsm.QuotedStrings.Internal;

/// <summary>
/// Where the scanner is.
/// </summary>
/// <remarks>
/// There is deliberately no "just finished a string" state. Closing a quote is something the machine
/// <em>does</em> (an effect), not somewhere it <em>is</em>; modelling it as a state forces a spurious
/// extra step that consumes the following character for nothing.
/// </remarks>
internal enum ScanState
{
    /// <summary>Between quoted runs. Everything here is prose and is discarded.</summary>
    Outside = 0,

    /// <summary>Inside a quoted run, accumulating its text.</summary>
    InQuotes = 1,

    /// <summary>The escape character was just consumed; whatever comes next is taken literally.</summary>
    AfterEscape = 2,

    /// <summary>The input ended cleanly.</summary>
    Completed = 3,

    /// <summary>The input was malformed; the reason is on the context.</summary>
    Failed = 4,
}

/// <summary>
/// The alphabet the table is indexed by.
/// </summary>
/// <remarks>
/// <see cref="EndOfInput"/> is a real symbol class rather than a special case in the driving loop.
/// That is what lets "the input ended inside a string" be an ordinary, declared transition instead
/// of a check bolted on after the loop, and it is the difference between reporting an unterminated
/// string and silently returning nothing.
/// </remarks>
internal enum CharClass
{
    /// <summary>Any character with no special meaning in the active syntax.</summary>
    Literal = 0,

    /// <summary>The character that opens and closes a quoted run.</summary>
    Quote = 1,

    /// <summary>The character that suppresses the next character's meaning.</summary>
    Escape = 2,

    /// <summary>The end of the input, injected once, after the last character.</summary>
    EndOfInput = 3,
}
