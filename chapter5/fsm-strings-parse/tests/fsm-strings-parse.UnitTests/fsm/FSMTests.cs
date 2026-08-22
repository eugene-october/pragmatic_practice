using fsm_strings_parse.fsm;

namespace fsm_strings_parse.UnitTests.fsm;

public class FSMTests
{
    [Fact]
    public void Process_QuotedText_ReturnsTextInsideQuotes()
    {
        // Arrange
        var fsm = new FSM();
        List<char> data = [];

        // Act
        foreach (var character in "\"hello\"")
        {
            FSMResult result = fsm.Process(character);

            if (result.Data is char token)
            {
                data.Add(token);
            }
        }

        // Assert
        Assert.NotNull(data);
        Assert.Equal("hello", new string(data.ToArray()));
    }

    [Fact]
    public void Process_EscapedText_ReturnsTextInsideQuotes()
    {
        // Arrange
        var textWithinQuotes = "START of super exciting text within a quote which eventually comes to an END";
        var textToBeParsed = $"contains a mega quote: \\\"{textWithinQuotes}\\\". Isn't it cool?";
        var asset = $"Hello, this is text, which \"{textToBeParsed}\" What do you think";
        var fsm = new FSM();
        List<char> data = [];

        // Act
        foreach (var character in asset)
        {
            FSMResult result = fsm.Process(character);

            if (result.Data is char token)
            {
                data.Add(token);
            }
        }

        // Assert
        var resultingText = textToBeParsed.Replace("\\", "");
        Assert.NotNull(data);
        Assert.Equal(resultingText, new string(data.ToArray()));
    }
}
