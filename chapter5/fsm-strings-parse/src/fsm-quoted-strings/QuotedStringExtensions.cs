namespace Fsm.QuotedStrings;

/// <summary>
/// The one-liners, for callers who want the text and nothing else.
/// </summary>
/// <remarks>
/// These deliberately discard <see cref="ScanError"/>, returning whatever was found before the
/// input went wrong. Use <see cref="QuotedStringScanner.Scan(ReadOnlySpan{char})"/> when it matters
/// whether the input was well formed.
/// </remarks>
public static class QuotedStringExtensions
{
    /// <summary>The text of the first quoted run, unescaped, or null when there is none.</summary>
    public static string? FirstQuoted(this string text, QuoteSyntax? syntax = null)
        => ScannerFor(syntax).Scan(text).FirstValue;

    /// <summary>The text of every quoted run, unescaped, in order.</summary>
    public static IReadOnlyList<string> AllQuoted(this string text, QuoteSyntax? syntax = null)
        => [.. ScannerFor(syntax).Scan(text).Values];

    private static QuotedStringScanner ScannerFor(QuoteSyntax? syntax)
        => syntax is null || ReferenceEquals(syntax, QuoteSyntax.Default)
            ? QuotedStringScanner.Default
            : new QuotedStringScanner(syntax);
}
