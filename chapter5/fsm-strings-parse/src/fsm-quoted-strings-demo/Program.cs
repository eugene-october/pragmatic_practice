using Fsm.QuotedStrings;

const string Sample = """Ololo "this is \"mega\" text". Nice, innit?""";
const string Broken = """A "well formed one" and then a "broken one that never ends""";
const string CStyle = """He said "line one\nline two" out loud.""";

var supplied = args.Length > 0
    ? string.Join(' ', args)
    : Console.IsInputRedirected ? Console.In.ReadToEnd() : null;

if (string.IsNullOrWhiteSpace(supplied))
{
    Describe("the example from the brief", Sample, QuotedStringScanner.Default);
    Describe("input that never closes its quote", Broken, QuotedStringScanner.Default);
    Describe("the same grammar, C style escapes", CStyle, new QuotedStringScanner(QuoteSyntax.CStyle));
}
else
{
    Describe("your input", supplied, QuotedStringScanner.Default);
}

Console.WriteLine();
Console.WriteLine("The grammar, generated from the very transition table those scans just ran:");
Console.WriteLine();
Console.WriteLine(QuotedStringScanner.MachineDiagram);

static void Describe(string caption, string text, QuotedStringScanner scanner)
{
    var result = scanner.Scan(text);

    Console.WriteLine();
    Console.WriteLine($"--- {caption} ---");
    Console.WriteLine($"  in   {Visible(text)}");

    foreach (var segment in result.Segments)
    {
        Console.WriteLine($"  out  {Visible(segment.Value)}  ({segment.Start} to {segment.End})");
    }

    if (result.Error is { } error)
    {
        Console.WriteLine($"  bad  {error.Kind}: {error.Message}");
    }
}

static string Visible(string text) => $"[{text.Replace("\n", "\\n").Replace("\t", "\\t")}]";
