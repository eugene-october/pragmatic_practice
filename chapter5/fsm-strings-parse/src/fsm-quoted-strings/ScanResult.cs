namespace Fsm.QuotedStrings;

/// <summary>
/// Everything a scan produced: the segments it recognised and, if the input was malformed, why.
/// </summary>
/// <remarks>
/// Segments found before a failure are still returned. Partial results are usually more useful than
/// none, and the caller can decide whether to trust them by checking <see cref="IsSuccess"/>.
/// </remarks>
public sealed record ScanResult(IReadOnlyList<QuotedSegment> Segments, ScanError? Error)
{
    /// <summary>True when the whole input was well formed.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>The unescaped text of every segment, in order.</summary>
    public IEnumerable<string> Values => Segments.Select(segment => segment.Value);

    /// <summary>The first segment's text, or null when nothing was quoted.</summary>
    public string? FirstValue => Segments.Count > 0 ? Segments[0].Value : null;
}
