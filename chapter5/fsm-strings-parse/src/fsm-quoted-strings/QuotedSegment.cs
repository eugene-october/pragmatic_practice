namespace Fsm.QuotedStrings;

/// <summary>
/// One quoted run found in the input, already unescaped.
/// </summary>
/// <param name="Value">The text between the quotes, with escape sequences resolved.</param>
/// <param name="Start">Position of the opening quote.</param>
/// <param name="End">Position of the closing quote.</param>
public readonly record struct QuotedSegment(string Value, TextPosition Start, TextPosition End)
{
    public override string ToString() => $"\"{Value}\" at {Start}";
}
