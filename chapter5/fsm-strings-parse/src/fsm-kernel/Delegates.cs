namespace Fsm.Kernel;

/// <summary>
/// Decides whether a guarded transition applies to the symbol currently being consumed.
/// </summary>
/// <remarks>
/// Guards must be side-effect free: the engine may evaluate several of them for a single symbol
/// and only the first one that returns <see langword="true"/> has its effect executed.
/// </remarks>
public delegate bool TransitionGuard<TSymbol, TContext>(in TSymbol symbol, TContext context);

/// <summary>
/// The side effect a transition performs while it is taken. This is the only place a machine is
/// allowed to touch the outside world, which keeps the transition table itself pure data.
/// </summary>
public delegate void TransitionEffect<TSymbol, TContext>(in TSymbol symbol, TContext context);
