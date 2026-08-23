namespace Fsm.QuotedStrings;

/// <summary>
/// Where something is in the input, in all three ways a caller might want it.
/// </summary>
/// <param name="Index">Zero based offset from the start of the input.</param>
/// <param name="Line">One based line number; a line ends at <c>\n</c>.</param>
/// <param name="Column">One based column within the line.</param>
public readonly record struct TextPosition(int Index, int Line, int Column)
{
    /// <summary>The position of the very first character of an input.</summary>
    public static TextPosition Start => new(0, 1, 1);

    public override string ToString() => $"line {Line}, column {Column}";
}
