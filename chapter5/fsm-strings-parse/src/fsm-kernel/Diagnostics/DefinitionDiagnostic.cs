namespace Fsm.Kernel.Diagnostics;

/// <summary>
/// The ways a declared state machine can be malformed. Every one of these is detected while the
/// machine is being built, so none of them can occur while it is running.
/// </summary>
public enum DiagnosticCode
{
    /// <summary><c>Initial(...)</c> was never called.</summary>
    NoInitialState,

    /// <summary>A state or symbol-class enum cannot index a dense table.</summary>
    NonIndexableEnum,

    /// <summary>A reachable, non-terminal state does not handle one of the symbol classes.</summary>
    MissingTransition,

    /// <summary>Two separate declarations claim the same state/symbol-class cell.</summary>
    DuplicateTransition,

    /// <summary>A cell was opened with <c>On(...)</c> but no destination was ever declared.</summary>
    NoTransitionDeclared,

    /// <summary>Every arm of a cell is guarded, so some symbol could fall through it.</summary>
    GuardWithoutFallback,

    /// <summary>An arm was declared after the unconditional fallback and can never be taken.</summary>
    TransitionAfterFallback,

    /// <summary>A state declared terminal also declares outgoing transitions.</summary>
    TerminalStateHasTransitions,

    /// <summary>A declared state cannot be reached from the initial state.</summary>
    UnreachableState,

    /// <summary>A declaration names an enum value that the enum does not declare.</summary>
    UndeclaredEnumValue,
}

/// <summary>A single problem found while validating a state machine definition.</summary>
public sealed record DefinitionDiagnostic(DiagnosticCode Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}
