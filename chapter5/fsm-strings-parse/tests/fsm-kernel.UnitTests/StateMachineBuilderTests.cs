using Fsm.Kernel;
using Fsm.Kernel.Diagnostics;

namespace Fsm.Kernel.UnitTests;

/// <summary>States for a deliberately tiny machine used to exercise the engine.</summary>
internal enum Light
{
    Red = 0,
    Green = 1,
    Off = 2,
}

internal enum Signal
{
    Go = 0,
    Stop = 1,
}

/// <summary>Not numbered from zero, so it cannot index a dense table.</summary>
internal enum Sparse
{
    First = 1,
    Second = 7,
}

public class StateMachineBuilderTests
{
    private static StateMachineBuilder<Light, Signal, char, List<string>> Declare()
        => StateMachine.For<Light, Signal, char, List<string>>();

    /// <summary>The machine that every "this is what correct looks like" test runs against.</summary>
    private static StateMachine<Light, Signal, char, List<string>> WellFormed()
        => Declare()
            .Initial(Light.Red)
            .Terminal(Light.Off)
            .From(Light.Red)
                .On(Signal.Go).Do(Effects.Record).GoTo(Light.Green)
                .On(Signal.Stop).Ignore().Stay()
            .From(Light.Green)
                .On(Signal.Stop).GoTo(Light.Off)
                .OnRemaining().Do(Effects.Record).Stay()
            .Build();

    [Fact]
    public void Build_WhenAStateIgnoresASymbolClass_ReportsThatExactHole()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .Terminal(Light.Green, Light.Off)
                .From(Light.Red)
                    .On(Signal.Go).GoTo(Light.Green)
                .Build());

        var hole = Assert.Single(thrown.Diagnostics);
        Assert.Equal(DiagnosticCode.MissingTransition, hole.Code);
        Assert.Contains("'Red'", hole.Message);
        Assert.Contains("'Stop'", hole.Message);
    }

    [Fact]
    public void Build_ReportsEveryProblemAtOnceRatherThanOnlyTheFirst()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Terminal(Light.Off)
                .From(Light.Red)
                    .On(Signal.Go).GoTo(Light.Green)
                .Build());

        // No initial state, plus Red/Stop, plus both of Green's cells.
        Assert.True(thrown.Has(DiagnosticCode.NoInitialState));
        Assert.Equal(3, thrown.Diagnostics.Count(diagnostic => diagnostic.Code == DiagnosticCode.MissingTransition));
        Assert.Contains("4 problems", thrown.Message);
    }

    [Fact]
    public void Build_WhenACellIsDeclaredTwice_RefusesToLetOneSilentlyWin()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .Terminal(Light.Green, Light.Off)
                .From(Light.Red)
                    .On(Signal.Go).GoTo(Light.Green)
                    .On(Signal.Stop).Stay()
                    .On(Signal.Go).GoTo(Light.Off)
                .Build());

        Assert.True(thrown.Has(DiagnosticCode.DuplicateTransition));
    }

    [Fact]
    public void Build_WhenEveryArmIsGuarded_ReportsThatASymbolCouldFallThrough()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .Terminal(Light.Green, Light.Off)
                .From(Light.Red)
                    .On(Signal.Go).When(Guards.IsDigit).GoTo(Light.Green)
                    .On(Signal.Stop).Stay()
                .Build());

        Assert.True(thrown.Has(DiagnosticCode.GuardWithoutFallback));
    }

    [Fact]
    public void Build_WhenAnArmIsAddedAfterTheFallback_ReportsItAsUnreachable()
    {
        var declaration = Declare().Initial(Light.Red).Terminal(Light.Green, Light.Off);
        var row = declaration.From(Light.Red);
        var cell = row.On(Signal.Go);

        cell.GoTo(Light.Green);
        cell.When(Guards.IsDigit).GoTo(Light.Off);
        row.On(Signal.Stop).Stay();

        var thrown = Assert.Throws<StateMachineDefinitionException>(() => declaration.Build());

        Assert.True(thrown.Has(DiagnosticCode.TransitionAfterFallback));
    }

    [Fact]
    public void Build_WhenACellNamesNoDestination_SaysSo()
    {
        var declaration = Declare().Initial(Light.Red).Terminal(Light.Green, Light.Off);
        var row = declaration.From(Light.Red);

        row.On(Signal.Go);
        row.On(Signal.Stop).Stay();

        var thrown = Assert.Throws<StateMachineDefinitionException>(() => declaration.Build());

        Assert.True(thrown.Has(DiagnosticCode.NoTransitionDeclared));
    }

    [Fact]
    public void Build_WhenAStateCannotBeReached_ReportsTheDeadRows()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .From(Light.Red)
                    .OnRemaining().Stay()
                .From(Light.Green)
                    .OnRemaining().Stay()
                .From(Light.Off)
                    .OnRemaining().Stay()
                .Build());

        Assert.Equal(2, thrown.Diagnostics.Count(diagnostic => diagnostic.Code == DiagnosticCode.UnreachableState));
    }

    [Fact]
    public void Build_WhenATerminalStateAlsoDeclaresTransitions_ReportsTheContradiction()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .Terminal(Light.Off)
                .From(Light.Red)
                    .On(Signal.Go).GoTo(Light.Green)
                    .On(Signal.Stop).GoTo(Light.Off)
                .From(Light.Green)
                    .OnRemaining().GoTo(Light.Off)
                .From(Light.Off)
                    .OnRemaining().Stay()
                .Build());

        Assert.True(thrown.Has(DiagnosticCode.TerminalStateHasTransitions));
    }

    [Fact]
    public void Build_WhenTheStateEnumIsNotDenselyNumbered_SaysWhyBeforeAnythingElse()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(
            () => StateMachine.For<Sparse, Signal, char, List<string>>().Build());

        var diagnostic = Assert.Single(thrown.Diagnostics);
        Assert.Equal(DiagnosticCode.NonIndexableEnum, diagnostic.Code);
        Assert.Contains("Sparse", diagnostic.Message);
    }

    [Fact]
    public void Build_WhenADeclarationNamesAValueTheEnumDoesNotDeclare_SaysSoInsteadOfCrashing()
    {
        var thrown = Assert.Throws<StateMachineDefinitionException>(() =>
            Declare()
                .Initial(Light.Red)
                .From(Light.Red)
                    .On(Signal.Go).GoTo((Light)99)
                    .On(Signal.Stop).Stay()
                .Build());

        var diagnostic = Assert.Single(thrown.Diagnostics);
        Assert.Equal(DiagnosticCode.UndeclaredEnumValue, diagnostic.Code);
        Assert.Contains("A transition destination", diagnostic.Message);
    }

    [Fact]
    public void OnRemaining_CoversClassesNamedLaterInTheDeclaration()
    {
        // Go is declared after OnRemaining, so declaration order must not decide who owns that cell.
        var machine = Declare()
            .Initial(Light.Red)
            .Terminal(Light.Off)
            .From(Light.Red)
                .OnRemaining().GoTo(Light.Off)
                .On(Signal.Go).GoTo(Light.Green)
            .From(Light.Green)
                .OnRemaining().GoTo(Light.Off)
            .Build();

        var log = new List<string>();

        Assert.Equal(Light.Green, machine.Step(Light.Red, Signal.Go, 'a', log));
        Assert.Equal(Light.Off, machine.Step(Light.Red, Signal.Stop, 'a', log));
    }

    [Fact]
    public void Step_TakesTheFirstArmWhoseGuardHolds()
    {
        var machine = GuardedMachine();
        var log = new List<string>();

        Assert.Equal(Light.Green, machine.Step(Light.Red, Signal.Go, '5', log));
        Assert.Equal(Light.Off, machine.Step(Light.Red, Signal.Go, 'x', log));
        Assert.Equal(new[] { "digit:5", "other:x" }, log);
    }

    [Fact]
    public void Step_DoesNotRunTheEffectOfAnArmWhoseGuardFailed()
    {
        var machine = GuardedMachine();
        var log = new List<string>();

        machine.Step(Light.Red, Signal.Go, 'x', log);

        Assert.Equal(new[] { "other:x" }, log);
    }

    [Fact]
    public void Step_OutOfATerminalState_IsRefused()
    {
        var machine = WellFormed();

        Assert.Throws<InvalidOperationException>(
            () => machine.Step(Light.Off, Signal.Go, 'a', new List<string>()));
    }

    [Fact]
    public void Step_WithAValueTheStateEnumDoesNotDeclare_IsRefused()
    {
        var machine = WellFormed();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => machine.Step((Light)99, Signal.Go, 'a', new List<string>()));
    }

    [Fact]
    public void Cursor_WalksFromTheInitialStateAndCanBeRewound()
    {
        var machine = WellFormed();
        var cursor = machine.CreateCursor();
        var log = new List<string>();

        Assert.Equal(Light.Red, cursor.State);
        Assert.False(cursor.IsTerminal);

        cursor.Advance(Signal.Go, 'a', log);
        Assert.Equal(Light.Green, cursor.State);

        cursor.Advance(Signal.Stop, 'b', log);
        Assert.Equal(Light.Off, cursor.State);
        Assert.True(cursor.IsTerminal);

        cursor.Reset();
        Assert.Equal(Light.Red, cursor.State);
    }

    [Fact]
    public void ToMermaid_DrawsTheTableThatWasActuallyBuilt()
    {
        var expected = string.Join(
            Environment.NewLine,
            "stateDiagram-v2",
            "    direction LR",
            "    [*] --> Red",
            "    Red --> Green : Go / Record",
            "    Red --> Red : Stop / ignore",
            "    Green --> Green : Go / Record",
            "    Green --> Off : Stop",
            "    Off --> [*]");

        Assert.Equal(expected, WellFormed().ToMermaid());
    }

    [Fact]
    public void ToMermaid_MergesArmsThatDifferOnlyBySymbolClass()
    {
        var machine = Declare()
            .Initial(Light.Red)
            .Terminal(Light.Off)
            .From(Light.Red)
                .OnRemaining().GoTo(Light.Green)
            .From(Light.Green)
                .OnRemaining().GoTo(Light.Off)
            .Build();

        Assert.Contains("Red --> Green : Go, Stop", machine.ToMermaid());
    }

    [Fact]
    public void Transitions_AreExposedInTableOrderSoTheDefinitionCanBeInspected()
    {
        Assert.Equal(
            new[]
            {
                "Red --Go / Record--> Green",
                "Red --Stop / ignore--> Red",
                "Green --Go / Record--> Green",
                "Green --Stop--> Off",
            },
            WellFormed().Transitions.Select(transition => transition.ToString()));
    }

    private static StateMachine<Light, Signal, char, List<string>> GuardedMachine()
        => Declare()
            .Initial(Light.Red)
            .Terminal(Light.Green, Light.Off)
            .From(Light.Red)
                .On(Signal.Go)
                    .When(Guards.IsDigit).Do(Effects.RecordDigit).GoTo(Light.Green)
                    .Otherwise().Do(Effects.RecordOther).GoTo(Light.Off)
                .On(Signal.Stop).GoTo(Light.Green)
            .Build();

    private static class Effects
    {
        internal static void Record(in char symbol, List<string> log) => log.Add(symbol.ToString());

        internal static void RecordDigit(in char symbol, List<string> log) => log.Add($"digit:{symbol}");

        internal static void RecordOther(in char symbol, List<string> log) => log.Add($"other:{symbol}");
    }

    private static class Guards
    {
        internal static bool IsDigit(in char symbol, List<string> log) => char.IsDigit(symbol);
    }
}
