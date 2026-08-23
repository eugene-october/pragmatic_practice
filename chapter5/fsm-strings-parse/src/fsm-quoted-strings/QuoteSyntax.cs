namespace Fsm.QuotedStrings;

/// <summary>
/// The three knobs that distinguish one quoting dialect from another.
/// </summary>
/// <remarks>
/// Everything dialect-specific lives here, which is why the transition table itself is a single
/// shared, immutable instance: the machine's shape does not depend on <em>which</em> character opens
/// a string, only on the fact that some character does.
/// </remarks>
public sealed record QuoteSyntax
{
    public QuoteSyntax(char quote, char escape, IEscapeDecoder escapeDecoder)
    {
        ArgumentNullException.ThrowIfNull(escapeDecoder);

        if (quote == escape)
        {
            throw new ArgumentException(
                $"The quote and escape characters must differ, but both are '{quote}'.",
                nameof(escape));
        }

        Quote = quote;
        Escape = escape;
        EscapeDecoder = escapeDecoder;
    }

    /// <summary>Opens and closes a quoted run.</summary>
    public char Quote { get; }

    /// <summary>Strips the special meaning from the character that follows it.</summary>
    public char Escape { get; }

    /// <summary>Turns an escaped character into the character it stands for.</summary>
    public IEscapeDecoder EscapeDecoder { get; }

    /// <summary>Double quotes, backslash escapes, every escape taken verbatim.</summary>
    public static QuoteSyntax Default { get; } = new('"', '\\', VerbatimEscapeDecoder.Instance);

    /// <summary>Double quotes, backslash escapes, C and JSON escape sequences.</summary>
    public static QuoteSyntax CStyle { get; } = new('"', '\\', CStyleEscapeDecoder.Instance);

    /// <summary>Single quotes, backslash escapes, every escape taken verbatim.</summary>
    public static QuoteSyntax SingleQuoted { get; } = new('\'', '\\', VerbatimEscapeDecoder.Instance);

    public override string ToString() => $"{Quote} ... {Quote} escaped with {Escape}";
}
