// See https://aka.ms/new-console-template for more information
using fsm_strings_parse.fsm;

Console.WriteLine("Hello, World!");

var text = """
  Ololo this is "START of Quoted text, which contains even \"escaped\" quotes with exactly expected END". Fantastic, right?
""";

var fsm = new FSM();

foreach (var ch in text)
{
    var processed = fsm.Process(ch);

    if (processed.Data is not null)
    {
        Console.WriteLine($"---processed.Data---{string.Join("", processed.Data)}");
    }
}
