namespace Fsm.QuotedStrings.UnitTests;

public class QuotedStringScannerTests
{
    /// <summary>The example the whole exercise is about.</summary>
    private const string Brief = """Ololo "this is \"mega\" text". Nice, innit?""";

    [Fact]
    public void Scan_TheExampleFromTheBrief_YieldsTheUnescapedText()
    {
        var result = QuotedStringScanner.Default.Scan(Brief);

        Assert.True(result.IsSuccess);
        Assert.Equal("""this is "mega" text""", Assert.Single(result.Segments).Value);
    }

    // Raw string literals below are deliberate: they perform no escape processing, so what is
    // written is exactly the text the scanner sees.
    [Theory]
    [InlineData("""x "plain" y""", "plain")]
    [InlineData("""x "" y""", "")]
    [InlineData("""x "an \"inner\" quote" y""", """an "inner" quote""")]
    [InlineData("""x "a\b" y""", "ab")]
    [InlineData("""x "a\\b" y""", """a\b""")]
    [InlineData("""x "ends with a slash\\" y""", """ends with a slash\""")]
    [InlineData("x \"\\\\\\\"\" y", "\\\"")]
    [InlineData("x \"quote at the very end \\\"\" y", "quote at the very end \"")]
    public void Scan_ResolvesEscapesVerbatimByDefault(string input, string expected)
    {
        var result = QuotedStringScanner.Default.Scan(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, Assert.Single(result.Segments).Value);
    }

    [Fact]
    public void Scan_TextWithNoQuotes_FindsNothingAndIsStillSuccessful()
    {
        var result = QuotedStringScanner.Default.Scan("nothing quoted at all, not even a backslash \\ here");

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Segments);
        Assert.Null(result.FirstValue);
    }

    [Fact]
    public void Scan_SeveralQuotedRuns_ReturnsThemInOrder()
    {
        var result = QuotedStringScanner.Default.Scan("""one "first" two "sec\"ond" three "" four""");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { "first", """sec"ond""", "" }, result.Values);
    }

    [Fact]
    public void Scan_AdjacentQuotedRuns_AreNotRunTogether()
    {
        var result = QuotedStringScanner.Default.Scan("""x "a""b" y""");

        Assert.Equal(new[] { "a", "b" }, result.Values);
    }

    [Fact]
    public void Scan_ReportsWhereASegmentStartsAndEnds()
    {
        var segment = Assert.Single(QuotedStringScanner.Default.Scan("""ab "cd" ef""").Segments);

        Assert.Equal(new TextPosition(3, 1, 4), segment.Start);
        Assert.Equal(new TextPosition(6, 1, 7), segment.End);
    }

    [Fact]
    public void Scan_TracksLinesAndColumnsAcrossNewlines()
    {
        var segment = Assert.Single(QuotedStringScanner.Default.Scan("first line\nsecond \"here\" line").Segments);

        Assert.Equal(2, segment.Start.Line);
        Assert.Equal(8, segment.Start.Column);
        Assert.Equal(2, segment.End.Line);
        Assert.Equal(13, segment.End.Column);
    }

    [Fact]
    public void Scan_AQuoteThatIsNeverClosed_IsReportedAgainstTheOpeningQuote()
    {
        var result = QuotedStringScanner.Default.Scan("""a "closed" and then "never closed""");

        Assert.False(result.IsSuccess);
        Assert.Equal(ScanErrorKind.UnterminatedString, result.Error!.Kind);
        Assert.Equal(new TextPosition(20, 1, 21), result.Error.Position);

        // Whatever was recognised before the failure is still worth returning.
        Assert.Equal(new[] { "closed" }, result.Values);
    }

    [Fact]
    public void Scan_InputEndingOnAnEscape_IsReportedAgainstTheEscape()
    {
        var result = QuotedStringScanner.Default.Scan("""he said "oh no\""");

        Assert.False(result.IsSuccess);
        Assert.Equal(ScanErrorKind.DanglingEscape, result.Error!.Kind);
        Assert.Equal(new TextPosition(14, 1, 15), result.Error.Position);
    }

    [Fact]
    public void Scan_AnEscapedClosingQuoteDoesNotCloseTheString()
    {
        var result = QuotedStringScanner.Default.Scan("""a "this \" never closes""");

        Assert.Equal(ScanErrorKind.UnterminatedString, result.Error!.Kind);
        Assert.Empty(result.Segments);
    }

    [Fact]
    public void Scan_WithCStyleEscapes_ResolvesTheSequences()
    {
        var result = new QuotedStringScanner(QuoteSyntax.CStyle).Scan("""say "one\ttwo\nthree\\four\"five" done""");

        Assert.True(result.IsSuccess);
        Assert.Equal("one\ttwo\nthree\\four\"five", Assert.Single(result.Segments).Value);
    }

    [Fact]
    public void Scan_WithCStyleEscapes_RejectsASequenceTheDialectDoesNotKnow()
    {
        var result = new QuotedStringScanner(QuoteSyntax.CStyle).Scan("""say "a\qb" done""");

        Assert.False(result.IsSuccess);
        Assert.Equal(ScanErrorKind.UnknownEscapeSequence, result.Error!.Kind);
        Assert.Equal(new TextPosition(6, 1, 7), result.Error.Position);
        Assert.Contains(@"\q", result.Error.Message);
    }

    [Fact]
    public void Scan_TheSameSequenceIsFineVerbatimAndRejectedInCStyle()
    {
        const string Input = """say "a\qb" done""";

        Assert.Equal("aqb", QuotedStringScanner.Default.Scan(Input).FirstValue);
        Assert.False(new QuotedStringScanner(QuoteSyntax.CStyle).Scan(Input).IsSuccess);
    }

    [Fact]
    public void Scan_WithSingleQuoteSyntax_TreatsDoubleQuotesAsOrdinaryText()
    {
        var result = new QuotedStringScanner(QuoteSyntax.SingleQuoted).Scan("""say 'a \'quoted\' "bit"' done""");

        Assert.True(result.IsSuccess);
        Assert.Equal("a 'quoted' \"bit\"", Assert.Single(result.Segments).Value);
    }

    [Fact]
    public void QuoteSyntax_WithTheSameQuoteAndEscapeCharacter_IsRefused()
        => Assert.Throws<ArgumentException>(() => new QuoteSyntax('"', '"', VerbatimEscapeDecoder.Instance));

    [Fact]
    public void MachineDiagram_DescribesTheGrammarThatIsActuallyRunning()
    {
        var diagram = QuotedStringScanner.MachineDiagram;

        Assert.StartsWith("stateDiagram-v2", diagram);
        Assert.Contains("Outside --> InQuotes : Quote / BeginSegment", diagram);
        Assert.Contains("InQuotes --> Failed : EndOfInput / ReportUnterminated", diagram);
        Assert.Contains("[CanDecode] / AppendDecoded", diagram);
    }
}
