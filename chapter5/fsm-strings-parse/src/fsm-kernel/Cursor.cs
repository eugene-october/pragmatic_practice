namespace Fsm.Kernel;

/// <summary>
/// A run in progress: the mutable "where am I" that pairs with an immutable
/// <see cref="StateMachine{TState,TClass,TSymbol,TContext}"/>.
/// </summary>
/// <remarks>
/// Kept as a <see langword="struct"/> so that driving a machine over a long input allocates nothing.
/// </remarks>
public struct Cursor<TState, TClass, TSymbol, TContext>
    where TState : struct, Enum
    where TClass : struct, Enum
{
    private readonly StateMachine<TState, TClass, TSymbol, TContext> _machine;

    internal Cursor(StateMachine<TState, TClass, TSymbol, TContext> machine)
    {
        _machine = machine;
        State = machine.InitialState;
    }

    public TState State { get; private set; }

    public readonly bool IsTerminal => _machine.IsTerminal(State);

    /// <summary>Consumes a symbol whose class the caller has already determined.</summary>
    public TState Advance(TClass on, in TSymbol symbol, TContext context)
        => State = _machine.Step(State, on, in symbol, context);

    /// <summary>
    /// Consumes a symbol, classifying it first. The classifier is passed through a generic
    /// constraint so a struct implementation is devirtualised rather than called through an interface.
    /// </summary>
    public TState Advance<TClassifier>(TClassifier classifier, in TSymbol symbol, TContext context)
        where TClassifier : ISymbolClassifier<TSymbol, TClass>
        => Advance(classifier.Classify(in symbol), in symbol, context);

    /// <summary>Returns the cursor to the machine's initial state.</summary>
    public void Reset() => State = _machine.InitialState;
}
