namespace Fsm.QuotedStrings;

/// <summary>
/// Decides what an escape sequence means.
/// </summary>
/// <remarks>
/// This is the one genuinely dialect-specific part of quoted-string parsing: JSON, C, shells and
/// CSV all agree on "a backslash escapes the next character" and disagree on what the pair means.
/// Keeping it behind an interface means a new dialect is a new decoder, not a new state machine.
/// </remarks>
public interface IEscapeDecoder
{
    /// <summary>
    /// Resolves the character that follows the escape character.
    /// </summary>
    /// <param name="escaped">The character that was escaped.</param>
    /// <param name="decoded">The character it stands for, when the sequence is recognised.</param>
    /// <returns>False when the sequence is not valid in this dialect.</returns>
    bool TryDecode(char escaped, out char decoded);
}

/// <summary>
/// Treats an escape as "the next character, verbatim": <c>\"</c> is a quote, <c>\n</c> is the
/// letter n. Nothing is ever rejected.
/// </summary>
/// <remarks>
/// This is the behaviour most people mean by "escaped quotes", and it is what
/// <see cref="QuoteSyntax.Default"/> uses.
/// </remarks>
public sealed class VerbatimEscapeDecoder : IEscapeDecoder
{
    public static VerbatimEscapeDecoder Instance { get; } = new();

    private VerbatimEscapeDecoder()
    {
    }

    public bool TryDecode(char escaped, out char decoded)
    {
        decoded = escaped;
        return true;
    }
}

/// <summary>
/// Resolves the C and JSON style escapes, and rejects anything else.
/// </summary>
public sealed class CStyleEscapeDecoder : IEscapeDecoder
{
    public static CStyleEscapeDecoder Instance { get; } = new();

    private CStyleEscapeDecoder()
    {
    }

    public bool TryDecode(char escaped, out char decoded)
    {
        (var recognised, decoded) = escaped switch
        {
            'n' => (true, '\n'),
            't' => (true, '\t'),
            'r' => (true, '\r'),
            '0' => (true, '\0'),
            'a' => (true, '\a'),
            'b' => (true, '\b'),
            'f' => (true, '\f'),
            'v' => (true, '\v'),
            '\\' => (true, '\\'),
            '"' => (true, '"'),
            '\'' => (true, '\''),
            '/' => (true, '/'),
            _ => (false, '\0'),
        };

        return recognised;
    }
}
