using System.Text;
using Fsm.Kernel.Internal;

namespace Fsm.Kernel;

/// <summary>
/// Renders a built table as a Mermaid <c>stateDiagram-v2</c>.
/// </summary>
/// <remarks>
/// Diagrams drawn by hand rot. This one is derived from the same array the engine steps through, so
/// it is correct by construction and stays correct as the declaration changes. Arms that differ only
/// by symbol class are merged into a single edge to keep the picture readable.
/// </remarks>
internal static class MermaidExporter
{
    internal static string Export<TState, TClass, TSymbol, TContext>(
        StateMachine<TState, TClass, TSymbol, TContext> machine)
        where TState : struct, Enum
        where TClass : struct, Enum
    {
        var edges = new List<Edge>();
        var index = new Dictionary<EdgeKey, Edge>();

        foreach (var transition in machine.Transitions)
        {
            var key = new EdgeKey(
                transition.From.ToString(),
                transition.To.ToString(),
                transition.Guard,
                transition.Effect,
                transition.GuardLabel,
                transition.EffectLabel);

            if (!index.TryGetValue(key, out var edge))
            {
                edge = new Edge(key);
                index.Add(key, edge);
                edges.Add(edge);
            }

            edge.Classes.Add(transition.On.ToString()!);
        }

        var diagram = new StringBuilder()
            .AppendLine("stateDiagram-v2")
            .AppendLine("    direction LR")
            .Append("    [*] --> ")
            .Append(machine.InitialState);

        foreach (var edge in edges)
        {
            diagram
                .AppendLine()
                .Append("    ")
                .Append(edge.Key.From)
                .Append(" --> ")
                .Append(edge.Key.To)
                .Append(" : ")
                .Append(edge.Label());
        }

        foreach (var state in EnumIndex<TState>.Values.Where(machine.IsTerminal))
        {
            diagram.AppendLine().Append("    ").Append(state).Append(" --> [*]");
        }

        return diagram.ToString();
    }

    private readonly record struct EdgeKey(
        string From,
        string To,
        Delegate? Guard,
        Delegate? Effect,
        string? GuardLabel,
        string? EffectLabel);

    private sealed class Edge(EdgeKey key)
    {
        internal EdgeKey Key { get; } = key;

        internal List<string> Classes { get; } = [];

        internal string Label()
        {
            var label = new StringBuilder(string.Join(", ", Classes));

            if (Key.GuardLabel is not null)
            {
                label.Append(" [").Append(Key.GuardLabel).Append(']');
            }

            if (Key.EffectLabel is not null)
            {
                label.Append(" / ").Append(Key.EffectLabel);
            }

            return label.ToString();
        }
    }
}
