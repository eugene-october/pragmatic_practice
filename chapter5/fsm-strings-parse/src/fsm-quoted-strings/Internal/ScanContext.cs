using System.Text;

namespace Fsm.QuotedStrings.Internal;

/// <summary>
/// Everything a single scan accumulates. The transition table is shared and immutable; this is the
/// only mutable thing in a run, and it is what the effects act on.
/// </summary>
internal sealed class ScanContext(QuoteSyntax syntax)
{
    private readonly StringBuilder _value = new();
    private readonly List<QuotedSegment> _segments = [];

    internal QuoteSyntax Syntax { get; } = syntax;

    /// <summary>Where the symbol currently being consumed sits in the input.</summary>
    internal TextPosition Position { get; set; } = TextPosition.Start;

    /// <summary>Where the open quote of the run in progress sits.</summary>
    internal TextPosition SegmentStart { get; private set; }

    /// <summary>Where the escape character of the sequence in progress sits.</summary>
    internal TextPosition EscapeStart { get; private set; }

    internal ScanError? Error { get; private set; }

    internal IReadOnlyList<QuotedSegment> Segments => _segments;

    internal void BeginSegment()
    {
        _value.Clear();
        SegmentStart = Position;
    }

    internal void MarkEscape() => EscapeStart = Position;

    internal void Append(char character) => _value.Append(character);

    internal void CompleteSegment()
    {
        _segments.Add(new QuotedSegment(_value.ToString(), SegmentStart, Position));
        _value.Clear();
    }

    /// <summary>
    /// Records why the input was rejected. Only the first failure is kept: the machine moves to a
    /// terminal state on failure, so a second one would be a bug rather than extra information.
    /// </summary>
    internal void Fail(ScanErrorKind kind, TextPosition position, string message)
        => Error ??= new ScanError(kind, position, message);
}
