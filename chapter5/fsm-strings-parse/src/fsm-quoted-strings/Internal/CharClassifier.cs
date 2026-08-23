using Fsm.Kernel;

namespace Fsm.QuotedStrings.Internal;

/// <summary>
/// Narrows a <see cref="char"/> to the handful of classes the table is indexed by.
/// </summary>
/// <remarks>
/// A readonly struct rather than a class: the cursor takes it through a generic constraint, so the
/// call is devirtualised and inlined instead of going through an interface dispatch per character.
/// The classifier never produces <see cref="CharClass.EndOfInput"/> — only the session can say the
/// input is over.
/// </remarks>
internal readonly struct CharClassifier(char quote, char escape) : ISymbolClassifier<char, CharClass>
{
    public CharClass Classify(in char symbol)
    {
        if (symbol == quote)
        {
            return CharClass.Quote;
        }

        return symbol == escape ? CharClass.Escape : CharClass.Literal;
    }
}
