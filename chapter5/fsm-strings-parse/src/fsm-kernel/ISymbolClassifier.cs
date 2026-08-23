namespace Fsm.Kernel;

/// <summary>
/// Narrows an alphabet of symbols (which may be huge, e.g. every <see cref="char"/>) down to a
/// small enum of symbol classes (e.g. quote / escape / literal).
/// </summary>
/// <remarks>
/// This narrowing is what lets the engine store transitions in a dense, directly indexed table
/// instead of a dictionary: the table has exactly <c>states x classes</c> cells.
/// Implement this as a <see langword="readonly struct"/> and pass it through a generic constraint
/// to let the JIT devirtualise <see cref="Classify"/>.
/// </remarks>
public interface ISymbolClassifier<TSymbol, TClass>
    where TClass : struct, Enum
{
    /// <summary>Maps a concrete symbol onto the class that drives the transition table.</summary>
    TClass Classify(in TSymbol symbol);
}
