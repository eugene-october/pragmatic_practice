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

            if (result.Data is null)
            {
                continue;
            }

            data.AddRange(result.Data);
        }

        // Assert
        Assert.NotNull(data);
        Assert.Equal("hello", new string(data.ToArray()));
    }
}
