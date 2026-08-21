using fsm_strings_parse.fsm;

namespace fsm_strings_parse.UnitTests.fsm;

public class FSMTests
{
    [Fact]
    public void Process_QuotedText_ReturnsTextInsideQuotes()
    {
        // Arrange
        var fsm = new FSM();
        FSMResult? result = null;

        // Act
        foreach (var character in "\"hello\"")
        {
            result = fsm.Process(character);
        }

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
        Assert.Equal("hello", new string(result.Data.ToArray()));
    }
}
