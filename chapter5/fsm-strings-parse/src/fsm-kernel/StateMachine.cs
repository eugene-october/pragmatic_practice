using Fsm.Kernel.Internal;

namespace Fsm.Kernel;

/// <summary>Entry point for declaring a state machine.</summary>
public static class StateMachine
{
    /// <summary>
    /// Starts a declaration for a machine over <typeparamref name="TState"/> driven by symbols of
    /// type <typeparamref name="TSymbol"/> narrowed to classes of type <typeparamref name="TClass"/>,
    /// with effects acting on a <typeparamref name="TContext"/>.
    /// </summary>
    public static StateMachineBuilder<TState, TClass, TSymbol, TContext> For<TState, TClass, TSymbol, TContext>()
        where TState : struct, Enum
        where TClass : struct, Enum
        => new();
}

/// <summary>
/// An immutable, validated transition table.
/// </summary>
/// <remarks>
/// <para>
/// A machine holds no execution state, so a single instance is safe to share across threads and
/// across any number of concurrent parses. Position within a run lives in a
/// <see cref="Cursor{TState,TClass,TSymbol,TContext}"/>.
/// </para>
/// <para>
/// The table is dense and indexed arithmetically, so <see cref="Step"/> costs one array lookup plus
/// at most one delegate call per guard. Nothing is allocated per symbol.
/// </para>
/// <para>
/// Because the builder proves the transition function total, <see cref="Step"/> has no
/// "invalid transition" failure mode: every reachable state handles every symbol class.
/// </para>
/// </remarks>
public sealed class StateMachine<TState, TClass, TSymbol, TContext>
    where TState : struct, Enum
    where TClass : struct, Enum
{
    private readonly Transition<TState, TClass, TSymbol, TContext>[][] _table;
    private readonly bool[] _isTerminal;
    private readonly int _classCount;

    internal StateMachine(
        TState initialState,
        Transition<TState, TClass, TSymbol, TContext>[][] table,
        bool[] isTerminal,
        int classCount,
        IReadOnlyList<Transition<TState, TClass, TSymbol, TContext>> transitions,
        IReadOnlyList<TState> terminalStates)
    {
        InitialState = initialState;
        _table = table;
        _isTerminal = isTerminal;
        _classCount = classCount;
        Transitions = transitions;
        TerminalStates = terminalStates;
    }

    public TState InitialState { get; }

    /// <summary>Every declared transition, ordered by state, then symbol class, then guard order.</summary>
    public IReadOnlyList<Transition<TState, TClass, TSymbol, TContext>> Transitions { get; }

    public IReadOnlyList<TState> TerminalStates { get; }

    /// <summary>A terminal state accepts no further symbols; it is where a run comes to rest.</summary>
    public bool IsTerminal(TState state) => _isTerminal[IndexOfState(state)];

    /// <summary>Creates a cursor positioned at <see cref="InitialState"/>.</summary>
    public Cursor<TState, TClass, TSymbol, TContext> CreateCursor() => new(this);

    /// <summary>
    /// Consumes one symbol: selects the first arm of the <c>(from, on)</c> cell whose guard holds,
    /// runs its effect, and returns the destination state.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="from"/> is terminal.</exception>
    public TState Step(TState from, TClass on, in TSymbol symbol, TContext context)
    {
        var stateIndex = IndexOfState(from);

        if (_isTerminal[stateIndex])
        {
            throw new InvalidOperationException(
                $"State '{from}' is terminal and accepts no further symbols.");
        }

        var arms = _table[(stateIndex * _classCount) + IndexOfClass(on)];

        foreach (var arm in arms)
        {
            if (arm.Guard is not null && !arm.Guard(in symbol, context))
            {
                continue;
            }

            arm.Effect?.Invoke(in symbol, context);
            return arm.To;
        }

        // Unreachable: the builder rejects any cell whose last arm is guarded.
        throw new InvalidOperationException(
            $"No arm of '{from}' / '{on}' accepted the symbol, which a validated table must not allow.");
    }

    /// <summary>Renders the table as a Mermaid <c>stateDiagram-v2</c>.</summary>
    /// <remarks>
    /// The diagram is generated from the table that is actually running, so documentation cannot
    /// drift away from behaviour.
    /// </remarks>
    public string ToMermaid() => MermaidExporter.Export(this);

    private static int IndexOfState(TState state)
    {
        var index = EnumIndex<TState>.ToIndex(state);
        if ((uint)index >= (uint)EnumIndex<TState>.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, $"'{typeof(TState).Name}' has no such member.");
        }

        return index;
    }

    private static int IndexOfClass(TClass symbolClass)
    {
        var index = EnumIndex<TClass>.ToIndex(symbolClass);
        if ((uint)index >= (uint)EnumIndex<TClass>.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(symbolClass), symbolClass, $"'{typeof(TClass).Name}' has no such member.");
        }

        return index;
    }
}
