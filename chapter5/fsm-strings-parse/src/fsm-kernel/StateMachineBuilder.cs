using Fsm.Kernel.Diagnostics;
using Fsm.Kernel.Internal;

namespace Fsm.Kernel;

/// <summary>
/// Declares a transition table as data and proves it well-formed before any symbol is consumed.
/// </summary>
/// <remarks>
/// <para>
/// The builder's contract is that a successfully built machine has a <em>total</em> transition
/// function: every reachable, non-terminal state handles every symbol class, and every cell ends in
/// an unguarded arm. That is why the running engine has no "unexpected input" branch — the case is
/// eliminated at construction rather than handled at run time.
/// </para>
/// <para>
/// Validation reports every problem at once, so an incomplete table is fixed in a single pass.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var machine = StateMachine.For&lt;ScanState, CharClass, char, ScanContext&gt;()
///     .Initial(ScanState.Outside)
///     .Terminal(ScanState.Completed, ScanState.Failed)
///     .From(ScanState.Outside)
///         .On(CharClass.Quote).Do(ScanActions.BeginSegment).GoTo(ScanState.InQuotes)
///         .On(CharClass.EndOfInput).GoTo(ScanState.Completed)
///         .OnRemaining().Ignore().Stay()
///     .Build();
/// </code>
/// </example>
public sealed class StateMachineBuilder<TState, TClass, TSymbol, TContext>
    where TState : struct, Enum
    where TClass : struct, Enum
{
    private readonly List<ArmGroup> _groups = [];
    private readonly List<TState> _terminals = [];
    private TState? _initial;

    internal StateMachineBuilder()
    {
    }

    /// <summary>Declares where a run begins. Required.</summary>
    public StateMachineBuilder<TState, TClass, TSymbol, TContext> Initial(TState state)
    {
        _initial = state;
        return this;
    }

    /// <summary>
    /// Declares states that end a run. Terminal states are exempt from the completeness rule and
    /// must not declare outgoing transitions.
    /// </summary>
    public StateMachineBuilder<TState, TClass, TSymbol, TContext> Terminal(params TState[] states)
    {
        _terminals.AddRange(states);
        return this;
    }

    /// <summary>Opens the row of the table for <paramref name="state"/>.</summary>
    public StateBuilder From(TState state) => new(this, state);

    /// <summary>Validates the declaration and freezes it into an immutable machine.</summary>
    /// <exception cref="StateMachineDefinitionException">The declaration is malformed.</exception>
    public StateMachine<TState, TClass, TSymbol, TContext> Build()
    {
        var diagnostics = new List<DefinitionDiagnostic>();

        // Indexability is checked first: without it nothing else can even be laid out.
        if (EnumIndex<TState>.Problem is { } stateProblem)
        {
            diagnostics.Add(new DefinitionDiagnostic(DiagnosticCode.NonIndexableEnum, stateProblem));
        }

        if (EnumIndex<TClass>.Problem is { } classProblem)
        {
            diagnostics.Add(new DefinitionDiagnostic(DiagnosticCode.NonIndexableEnum, classProblem));
        }

        if (diagnostics.Count > 0)
        {
            throw new StateMachineDefinitionException(diagnostics);
        }

        var stateCount = EnumIndex<TState>.Count;
        var classCount = EnumIndex<TClass>.Count;

        // Every value that will be used as an array index has to be a real member, or laying the
        // table out would fail with an index-out-of-range rather than a diagnostic.
        ValidateDeclaredValues(diagnostics, stateCount, classCount);

        if (diagnostics.Count > 0)
        {
            throw new StateMachineDefinitionException(diagnostics);
        }

        if (_initial is null)
        {
            diagnostics.Add(new DefinitionDiagnostic(
                DiagnosticCode.NoInitialState,
                "No initial state was declared; call Initial(...)."));
        }

        var isTerminal = new bool[stateCount];
        foreach (var terminal in _terminals)
        {
            isTerminal[EnumIndex<TState>.ToIndex(terminal)] = true;
        }

        // OnRemaining() means "whatever this state does not name explicitly", so explicit coverage
        // has to be known before any group is expanded, regardless of declaration order.
        var namedExplicitly = new bool[stateCount * classCount];
        foreach (var group in _groups.Where(group => group.Classes is not null))
        {
            foreach (var symbolClass in group.Classes!)
            {
                namedExplicitly[CellOf(group.From, symbolClass, classCount)] = true;
            }
        }

        var cells = new Transition<TState, TClass, TSymbol, TContext>[stateCount * classCount][];

        foreach (var group in _groups)
        {
            ValidateArms(group, diagnostics);

            foreach (var symbolClass in ResolveClasses(group, namedExplicitly, classCount))
            {
                var cell = CellOf(group.From, symbolClass, classCount);

                if (cells[cell] is not null)
                {
                    diagnostics.Add(new DefinitionDiagnostic(
                        DiagnosticCode.DuplicateTransition,
                        $"State '{group.From}' declares symbol class '{symbolClass}' more than once."));
                    continue;
                }

                cells[cell] = [.. group.Arms.Select(arm => arm.ToTransition(group.From, symbolClass))];
            }
        }

        ValidateCompleteness(cells, isTerminal, stateCount, classCount, diagnostics);

        // Reachability is only meaningful once the table is otherwise sound; running it on a table
        // with holes produces cascades of misleading noise.
        if (diagnostics.Count == 0)
        {
            ValidateReachability(cells, _initial!.Value, stateCount, classCount, diagnostics);
        }

        if (diagnostics.Count > 0)
        {
            throw new StateMachineDefinitionException(diagnostics);
        }

        var table = new Transition<TState, TClass, TSymbol, TContext>[stateCount * classCount][];
        var ordered = new List<Transition<TState, TClass, TSymbol, TContext>>();

        for (var state = 0; state < stateCount; state++)
        {
            for (var symbolClass = 0; symbolClass < classCount; symbolClass++)
            {
                var cell = (state * classCount) + symbolClass;
                table[cell] = cells[cell] ?? [];
                ordered.AddRange(table[cell]);
            }
        }

        return new StateMachine<TState, TClass, TSymbol, TContext>(
            _initial!.Value,
            table,
            isTerminal,
            classCount,
            ordered,
            [.. _terminals.Distinct()]);
    }

    private void ValidateDeclaredValues(List<DefinitionDiagnostic> diagnostics, int stateCount, int classCount)
    {
        void CheckState(TState state, string role)
        {
            var index = EnumIndex<TState>.ToIndex(state);
            if ((uint)index >= (uint)stateCount)
            {
                diagnostics.Add(new DefinitionDiagnostic(
                    DiagnosticCode.UndeclaredEnumValue,
                    $"{role} uses {index}, which '{typeof(TState).Name}' does not declare."));
            }
        }

        void CheckClass(TClass symbolClass, string role)
        {
            var index = EnumIndex<TClass>.ToIndex(symbolClass);
            if ((uint)index >= (uint)classCount)
            {
                diagnostics.Add(new DefinitionDiagnostic(
                    DiagnosticCode.UndeclaredEnumValue,
                    $"{role} uses {index}, which '{typeof(TClass).Name}' does not declare."));
            }
        }

        if (_initial is { } initial)
        {
            CheckState(initial, "The initial state");
        }

        foreach (var terminal in _terminals)
        {
            CheckState(terminal, "A terminal state");
        }

        foreach (var group in _groups)
        {
            CheckState(group.From, "A From(...) row");

            foreach (var symbolClass in group.Classes ?? [])
            {
                CheckClass(symbolClass, "An On(...) symbol class");
            }

            foreach (var arm in group.Arms)
            {
                CheckState(arm.To, "A transition destination");
            }
        }
    }

    private static void ValidateArms(ArmGroup group, List<DefinitionDiagnostic> diagnostics)
    {
        var subject = group.Classes is null
            ? $"State '{group.From}' / remaining symbol classes"
            : $"State '{group.From}' / {string.Join(", ", group.Classes.Select(symbolClass => $"'{symbolClass}'"))}";

        if (group.Arms.Count == 0)
        {
            diagnostics.Add(new DefinitionDiagnostic(
                DiagnosticCode.NoTransitionDeclared,
                $"{subject} names no destination; finish the declaration with GoTo(...) or Stay()."));
            return;
        }

        for (var i = 0; i < group.Arms.Count - 1; i++)
        {
            if (group.Arms[i].Guard is null)
            {
                diagnostics.Add(new DefinitionDiagnostic(
                    DiagnosticCode.TransitionAfterFallback,
                    $"{subject} declares an arm after its unguarded fallback; that arm can never be taken."));
            }
        }

        if (group.Arms[^1].Guard is not null)
        {
            diagnostics.Add(new DefinitionDiagnostic(
                DiagnosticCode.GuardWithoutFallback,
                $"{subject} is guarded all the way down; add Otherwise() so every symbol has a destination."));
        }
    }

    private static void ValidateCompleteness(
        Transition<TState, TClass, TSymbol, TContext>[]?[] cells,
        bool[] isTerminal,
        int stateCount,
        int classCount,
        List<DefinitionDiagnostic> diagnostics)
    {
        for (var state = 0; state < stateCount; state++)
        {
            var declaresAnything = false;
            for (var symbolClass = 0; symbolClass < classCount; symbolClass++)
            {
                if (cells[(state * classCount) + symbolClass] is not null)
                {
                    declaresAnything = true;
                    break;
                }
            }

            if (isTerminal[state])
            {
                if (declaresAnything)
                {
                    diagnostics.Add(new DefinitionDiagnostic(
                        DiagnosticCode.TerminalStateHasTransitions,
                        $"State '{EnumIndex<TState>.FromIndex(state)}' is terminal but declares outgoing transitions."));
                }

                continue;
            }

            for (var symbolClass = 0; symbolClass < classCount; symbolClass++)
            {
                if (cells[(state * classCount) + symbolClass] is not null)
                {
                    continue;
                }

                diagnostics.Add(new DefinitionDiagnostic(
                    DiagnosticCode.MissingTransition,
                    $"State '{EnumIndex<TState>.FromIndex(state)}' does not handle symbol class "
                    + $"'{EnumIndex<TClass>.FromIndex(symbolClass)}'."));
            }
        }
    }

    private static void ValidateReachability(
        Transition<TState, TClass, TSymbol, TContext>[]?[] cells,
        TState initial,
        int stateCount,
        int classCount,
        List<DefinitionDiagnostic> diagnostics)
    {
        var reachable = new bool[stateCount];
        var pending = new Queue<int>();

        var start = EnumIndex<TState>.ToIndex(initial);
        reachable[start] = true;
        pending.Enqueue(start);

        while (pending.Count > 0)
        {
            var state = pending.Dequeue();

            for (var symbolClass = 0; symbolClass < classCount; symbolClass++)
            {
                foreach (var arm in cells[(state * classCount) + symbolClass] ?? [])
                {
                    var destination = EnumIndex<TState>.ToIndex(arm.To);
                    if (reachable[destination])
                    {
                        continue;
                    }

                    reachable[destination] = true;
                    pending.Enqueue(destination);
                }
            }
        }

        for (var state = 0; state < stateCount; state++)
        {
            if (reachable[state])
            {
                continue;
            }

            diagnostics.Add(new DefinitionDiagnostic(
                DiagnosticCode.UnreachableState,
                $"State '{EnumIndex<TState>.FromIndex(state)}' cannot be reached from '{initial}'."));
        }
    }

    private static IEnumerable<TClass> ResolveClasses(ArmGroup group, bool[] namedExplicitly, int classCount)
    {
        if (group.Classes is not null)
        {
            return group.Classes;
        }

        var offset = EnumIndex<TState>.ToIndex(group.From) * classCount;

        return Enumerable
            .Range(0, classCount)
            .Where(symbolClass => !namedExplicitly[offset + symbolClass])
            .Select(EnumIndex<TClass>.FromIndex)
            .ToArray();
    }

    private static int CellOf(TState state, TClass symbolClass, int classCount)
        => (EnumIndex<TState>.ToIndex(state) * classCount) + EnumIndex<TClass>.ToIndex(symbolClass);

    /// <summary>
    /// Names a delegate after its method so diagrams label themselves. Lambdas get compiler
    /// generated names, which are worse than nothing, so those fall back to the caller's label.
    /// </summary>
    private static string? Describe(Delegate handler)
    {
        var name = handler.Method.Name;
        return name.StartsWith('<') ? null : name;
    }

    internal sealed class ArmGroup(TState from, TClass[]? classes)
    {
        /// <summary>Null means "every symbol class this state does not name explicitly".</summary>
        internal TClass[]? Classes { get; } = classes;

        internal TState From { get; } = from;

        internal List<ArmDraft> Arms { get; } = [];
    }

    internal sealed class ArmDraft
    {
        internal TransitionGuard<TSymbol, TContext>? Guard { get; init; }

        internal string? GuardLabel { get; init; }

        internal TransitionEffect<TSymbol, TContext>? Effect { get; init; }

        internal string? EffectLabel { get; init; }

        internal TState To { get; init; }

        internal Transition<TState, TClass, TSymbol, TContext> ToTransition(TState from, TClass on)
            => new(from, on, To, Guard, GuardLabel, Effect, EffectLabel);
    }

    /// <summary>One row of the table: everything the machine does while in a single state.</summary>
    public sealed class StateBuilder
    {
        private readonly StateMachineBuilder<TState, TClass, TSymbol, TContext> _owner;
        private readonly TState _state;

        internal StateBuilder(StateMachineBuilder<TState, TClass, TSymbol, TContext> owner, TState state)
        {
            _owner = owner;
            _state = state;
        }

        /// <summary>Opens the cell for one symbol class.</summary>
        public ArmBuilder On(TClass symbolClass) => Open([symbolClass]);

        /// <summary>Opens several cells that share the same arms.</summary>
        public ArmBuilder OnAny(params TClass[] symbolClasses) => Open(symbolClasses);

        /// <summary>
        /// Opens the cells for every symbol class this state does not name explicitly, wherever
        /// those <c>On(...)</c> declarations appear.
        /// </summary>
        public ArmBuilder OnRemaining() => Open(null);

        /// <summary>Moves on to another row.</summary>
        public StateBuilder From(TState state) => _owner.From(state);

        /// <inheritdoc cref="StateMachineBuilder{TState,TClass,TSymbol,TContext}.Build"/>
        public StateMachine<TState, TClass, TSymbol, TContext> Build() => _owner.Build();

        private ArmBuilder Open(TClass[]? symbolClasses)
        {
            var group = new ArmGroup(_state, symbolClasses);
            _owner._groups.Add(group);
            return new ArmBuilder(this, _state, group);
        }
    }

    /// <summary>The arms of one cell, tried in declaration order.</summary>
    public sealed class ArmBuilder
    {
        private readonly ArmGroup _group;
        private readonly StateBuilder _parent;
        private readonly TState _state;
        private TransitionEffect<TSymbol, TContext>? _effect;
        private string? _effectLabel;

        internal ArmBuilder(StateBuilder parent, TState state, ArmGroup group)
        {
            _parent = parent;
            _state = state;
            _group = group;
        }

        /// <summary>Adds a conditional arm, tried before any arm declared after it.</summary>
        public GuardedArmBuilder When(TransitionGuard<TSymbol, TContext> guard, string? label = null)
            => new(this, _state, _group, guard, label ?? Describe(guard) ?? "guard");

        /// <summary>Prose only: marks the unconditional arm that closes a guarded cell.</summary>
        public ArmBuilder Otherwise() => this;

        /// <summary>Attaches the side effect this arm performs.</summary>
        public ArmBuilder Do(TransitionEffect<TSymbol, TContext> effect, string? label = null)
        {
            _effect = effect;
            _effectLabel = label ?? Describe(effect) ?? "effect";
            return this;
        }

        /// <summary>States outright that the symbol is discarded, which a diagram then shows.</summary>
        public ArmBuilder Ignore()
        {
            _effect = null;
            _effectLabel = "ignore";
            return this;
        }

        /// <summary>Closes the cell with its unconditional destination.</summary>
        public StateBuilder GoTo(TState destination)
        {
            _group.Arms.Add(new ArmDraft
            {
                Effect = _effect,
                EffectLabel = _effectLabel,
                To = destination,
            });

            _effect = null;
            _effectLabel = null;

            return _parent;
        }

        /// <summary>Closes the cell with a self-loop.</summary>
        public StateBuilder Stay() => GoTo(_state);

        // A cell that never receives its fallback is a mistake, but it has to be *possible* to
        // write, otherwise the diagnostic that catches it could never fire. These forward to the
        // enclosing row so the fluent chain can walk away from an unfinished cell, and Build() is
        // left to object.

        /// <inheritdoc cref="StateBuilder.On"/>
        public ArmBuilder On(TClass symbolClass) => _parent.On(symbolClass);

        /// <inheritdoc cref="StateBuilder.OnAny"/>
        public ArmBuilder OnAny(params TClass[] symbolClasses) => _parent.OnAny(symbolClasses);

        /// <inheritdoc cref="StateBuilder.OnRemaining"/>
        public ArmBuilder OnRemaining() => _parent.OnRemaining();

        /// <inheritdoc cref="StateBuilder.From"/>
        public StateBuilder From(TState state) => _parent.From(state);

        /// <inheritdoc cref="StateMachineBuilder{TState,TClass,TSymbol,TContext}.Build"/>
        public StateMachine<TState, TClass, TSymbol, TContext> Build() => _parent.Build();
    }

    /// <summary>A single conditional arm, awaiting its destination.</summary>
    public sealed class GuardedArmBuilder
    {
        private readonly ArmGroup _group;
        private readonly TransitionGuard<TSymbol, TContext> _guard;
        private readonly string _guardLabel;
        private readonly ArmBuilder _parent;
        private readonly TState _state;
        private TransitionEffect<TSymbol, TContext>? _effect;
        private string? _effectLabel;

        internal GuardedArmBuilder(
            ArmBuilder parent,
            TState state,
            ArmGroup group,
            TransitionGuard<TSymbol, TContext> guard,
            string guardLabel)
        {
            _parent = parent;
            _state = state;
            _group = group;
            _guard = guard;
            _guardLabel = guardLabel;
        }

        /// <inheritdoc cref="ArmBuilder.Do"/>
        public GuardedArmBuilder Do(TransitionEffect<TSymbol, TContext> effect, string? label = null)
        {
            _effect = effect;
            _effectLabel = label ?? Describe(effect) ?? "effect";
            return this;
        }

        /// <inheritdoc cref="ArmBuilder.Ignore"/>
        public GuardedArmBuilder Ignore()
        {
            _effect = null;
            _effectLabel = "ignore";
            return this;
        }

        /// <summary>Closes this arm and returns to the cell so it can be given a fallback.</summary>
        public ArmBuilder GoTo(TState destination)
        {
            _group.Arms.Add(new ArmDraft
            {
                Guard = _guard,
                GuardLabel = _guardLabel,
                Effect = _effect,
                EffectLabel = _effectLabel,
                To = destination,
            });

            return _parent;
        }

        /// <summary>Closes this arm with a self-loop.</summary>
        public ArmBuilder Stay() => GoTo(_state);
    }
}
