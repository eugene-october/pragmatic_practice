namespace Fsm.QuotedStrings.UnitTests;

public class ScanSessionTests
{
    private const string Mixed = """Ololo "this is \"mega\" text" and "another \\ one" done""";

    [Fact]
    public void Feeding_InEveryPossibleSplit_GivesTheSameAnswerAsFeedingItWhole()
    {
        var whole = QuotedStringScanner.Default.Scan(Mixed);

        for (var split = 0; split <= Mixed.Length; split++)
        {
            var session = QuotedStringScanner.Default.BeginSession();
            session.Feed(Mixed.AsSpan(0, split));
            session.Feed(Mixed.AsSpan(split));

            var chunked = session.Complete();

            Assert.Equal(whole.Segments, chunked.Segments);
            Assert.Equal(whole.Error, chunked.Error);
        }
    }

    [Fact]
    public void Feeding_OneCharacterAtATime_GivesTheSameAnswer()
    {
        var session = QuotedStringScanner.Default.BeginSession();

        foreach (var character in Mixed)
        {
            session.Feed(character);
        }

        Assert.Equal(QuotedStringScanner.Default.Scan(Mixed).Segments, session.Complete().Segments);
    }

    [Fact]
    public void Scan_ThroughATextReaderWithATinyBuffer_CrossesEveryBufferBoundaryCorrectly()
    {
        var repeated = string.Concat(Enumerable.Repeat("""prose "seg\"ment" more """, 200));

        using var reader = new StringReader(repeated);
        var streamed = QuotedStringScanner.Default.Scan(reader, bufferSize: 3);

        Assert.True(streamed.IsSuccess);
        Assert.Equal(200, streamed.Segments.Count);
        Assert.All(streamed.Values, value => Assert.Equal("""seg"ment""", value));
    }

    [Fact]
    public void Scan_ThroughATextReader_TracksPositionsAcrossTheWholeStream()
    {
        using var reader = new StringReader("line one\nline \"two\"\nline three");
        var segment = Assert.Single(QuotedStringScanner.Default.Scan(reader, bufferSize: 4).Segments);

        Assert.Equal(new TextPosition(14, 2, 6), segment.Start);
        Assert.Equal(new TextPosition(18, 2, 10), segment.End);
    }

    [Fact]
    public void Complete_IsIdempotent()
    {
        var session = QuotedStringScanner.Default.BeginSession();
        session.Feed("""a "b" c""");

        var first = session.Complete();

        Assert.Same(first, session.Complete());
        Assert.True(session.IsCompleted);
    }

    [Fact]
    public void Feed_AfterComplete_IsRefusedRatherThanSilentlyIgnored()
    {
        var session = QuotedStringScanner.Default.BeginSession();
        session.Complete();

        Assert.Throws<InvalidOperationException>(() => session.Feed("more"));
    }

    [Fact]
    public void Feed_AfterTheInputWasFoundMalformed_StopsConsuming()
    {
        var session = QuotedStringScanner.Default.BeginSession();
        session.Feed("""a "never closed""");
        session.Feed('\\');

        var result = session.Complete();

        Assert.Equal(ScanErrorKind.DanglingEscape, result.Error!.Kind);
        Assert.True(session.IsFaulted);
    }

    [Fact]
    public void FirstQuoted_IsTheOneLinerForTheCommonCase()
        => Assert.Equal("""this is "mega" text""", """Ololo "this is \"mega\" text". Nice, innit?""".FirstQuoted());

    [Fact]
    public void FirstQuoted_WhenNothingIsQuoted_IsNull()
        => Assert.Null("nothing here".FirstQuoted());

    [Fact]
    public void AllQuoted_ReturnsEveryRun()
        => Assert.Equal(new[] { "one", "two" }, """a "one" b "two" c""".AllQuoted());

    [Fact]
    public void AllQuoted_HonoursAnAlternativeSyntax()
        => Assert.Equal(new[] { "one" }, """a 'one' b "not this" c""".AllQuoted(QuoteSyntax.SingleQuoted));

    [Fact]
    public void SessionsAreIndependentSoTheSharedMachineCanBeUsedConcurrently()
    {
        var inputs = Enumerable.Range(0, 500).Select(index => $"""noise "value {index}" tail""").ToArray();

        var results = new string?[inputs.Length];
        Parallel.For(0, inputs.Length, index => results[index] = QuotedStringScanner.Default.Scan(inputs[index]).FirstValue);

        Assert.Equal(Enumerable.Range(0, inputs.Length).Select(index => $"value {index}"), results);
    }
}
