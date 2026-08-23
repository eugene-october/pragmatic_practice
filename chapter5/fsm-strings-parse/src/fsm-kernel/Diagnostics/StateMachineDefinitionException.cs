using System.Text;

namespace Fsm.Kernel.Diagnostics;

/// <summary>
/// Thrown by <c>Build()</c> when a declared machine is malformed.
/// </summary>
/// <remarks>
/// The exception reports <em>every</em> problem found, not just the first, so a mis-declared table
/// can be fixed in one pass instead of one round trip per hole.
/// </remarks>
public sealed class StateMachineDefinitionException : Exception
{
    public StateMachineDefinitionException(IReadOnlyList<DefinitionDiagnostic> diagnostics)
        : base(Format(diagnostics))
    {
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<DefinitionDiagnostic> Diagnostics { get; }

    /// <summary>True when any reported problem carries the given code.</summary>
    public bool Has(DiagnosticCode code) => Diagnostics.Any(diagnostic => diagnostic.Code == code);

    private static string Format(IReadOnlyList<DefinitionDiagnostic> diagnostics)
    {
        var builder = new StringBuilder()
            .Append("The state machine definition is invalid (")
            .Append(diagnostics.Count)
            .Append(diagnostics.Count == 1 ? " problem):" : " problems):");

        foreach (var diagnostic in diagnostics)
        {
            builder.AppendLine().Append("  - ").Append(diagnostic);
        }

        return builder.ToString();
    }
}
