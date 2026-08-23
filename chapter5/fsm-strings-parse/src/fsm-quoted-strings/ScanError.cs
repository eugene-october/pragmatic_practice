namespace Fsm.QuotedStrings;

/// <summary>The ways an input can be malformed.</summary>
public enum ScanErrorKind
{
    /// <summary>A quoted run was opened and the input ended before it was closed.</summary>
    UnterminatedString,

    /// <summary>The input ended immediately after an escape character.</summary>
    DanglingEscape,

    /// <summary>The active <see cref="IEscapeDecoder"/> does not recognise the escape sequence.</summary>
    UnknownEscapeSequence,
}

/// <summary>
/// A malformed input, described rather than thrown.
/// </summary>
/// <remarks>
/// Bad input is an expected outcome of parsing, not an exceptional one, so it is returned as data
/// on <see cref="ScanResult"/>. The position points at the construct that <em>started</em> the
/// problem (the unclosed quote, the orphaned escape) rather than at the end of the input, because
/// that is where the reader has to go to fix it.
/// </remarks>
public sealed record ScanError(ScanErrorKind Kind, TextPosition Position, string Message)
{
    public override string ToString() => $"{Message} ({Position})";
}
