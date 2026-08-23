using System.Buffers;
using Fsm.QuotedStrings.Internal;

namespace Fsm.QuotedStrings;

/// <summary>
/// Pulls quoted, escape-aware runs of text out of arbitrary input.
/// </summary>
/// <example>
/// <code>
/// var result = QuotedStringScanner.Default.Scan("""Ololo "this is \"mega\" text". Nice, innit?""");
/// // result.FirstValue == "this is \"mega\" text"
/// </code>
/// </example>
/// <remarks>
/// Immutable and therefore thread safe; a single instance can serve a whole application. Per-run
/// state lives in the <see cref="ScanSession"/> that each call creates.
/// </remarks>
public sealed class QuotedStringScanner
{
    private const int DefaultBufferSize = 4096;

    public QuotedStringScanner(QuoteSyntax? syntax = null) => Syntax = syntax ?? QuoteSyntax.Default;

    /// <summary>Double quotes with backslash escapes.</summary>
    public static QuotedStringScanner Default { get; } = new(QuoteSyntax.Default);

    public QuoteSyntax Syntax { get; }

    /// <summary>
    /// The grammar this scanner runs, as a Mermaid <c>stateDiagram-v2</c>, generated from the live
    /// transition table.
    /// </summary>
    public static string MachineDiagram => ScannerMachine.Instance.ToMermaid();

    /// <summary>Starts an incremental scan, for input that arrives in pieces.</summary>
    public ScanSession BeginSession() => new(Syntax);

    /// <summary>Scans text already held in memory.</summary>
    public ScanResult Scan(ReadOnlySpan<char> text)
    {
        var session = BeginSession();
        session.Feed(text);
        return session.Complete();
    }

    /// <summary>
    /// Scans a stream without ever holding all of it, reading through a pooled buffer.
    /// </summary>
    public ScanResult Scan(TextReader reader, int bufferSize = DefaultBufferSize)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentOutOfRangeException.ThrowIfLessThan(bufferSize, 1);

        var session = BeginSession();
        var buffer = ArrayPool<char>.Shared.Rent(bufferSize);

        try
        {
            int read;
            while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                session.Feed(buffer.AsSpan(0, read));
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }

        return session.Complete();
    }
}
