namespace Fsm.Kernel;

/// <summary>
/// One row of the transition table: "while in <see cref="From"/>, seeing <see cref="On"/>
/// (and satisfying <see cref="Guard"/>), run <see cref="Effect"/> and move to <see cref="To"/>".
/// </summary>
/// <remarks>
/// Transitions are immutable and are shared by every machine instance built from a definition.
/// </remarks>
public sealed class Transition<TState, TClass, TSymbol, TContext>
    where TState : struct, Enum
    where TClass : struct, Enum
{
    internal Transition(
        TState from,
        TClass on,
        TState to,
        TransitionGuard<TSymbol, TContext>? guard,
        string? guardLabel,
        TransitionEffect<TSymbol, TContext>? effect,
        string? effectLabel)
    {
        From = from;
        On = on;
        To = to;
        Guard = guard;
        GuardLabel = guardLabel;
        Effect = effect;
        EffectLabel = effectLabel;
    }

    public TState From { get; }

    public TClass On { get; }

    public TState To { get; }

    /// <summary>Null for the fallback arm of a cell, which every cell is required to have.</summary>
    public TransitionGuard<TSymbol, TContext>? Guard { get; }

    public TransitionEffect<TSymbol, TContext>? Effect { get; }

    /// <summary>Display name for <see cref="Guard"/>, derived from the method name where possible.</summary>
    public string? GuardLabel { get; }

    /// <summary>
    /// Display name for <see cref="Effect"/>. Also set for an explicitly ignored symbol, where the
    /// absence of an effect is itself worth showing in a diagram.
    /// </summary>
    public string? EffectLabel { get; }

    /// <summary>True when this arm applies unconditionally.</summary>
    public bool IsFallback => Guard is null;

    public override string ToString()
    {
        var guard = GuardLabel is null ? string.Empty : $" [{GuardLabel}]";
        var effect = EffectLabel is null ? string.Empty : $" / {EffectLabel}";
        return $"{From} --{On}{guard}{effect}--> {To}";
    }
}
