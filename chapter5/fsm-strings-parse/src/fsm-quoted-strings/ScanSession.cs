using Fsm.Kernel;
using Fsm.QuotedStrings.Internal;

namespace Fsm.QuotedStrings;

/// <summary>
/// A scan in progress, fed one chunk at a time.
/// </summary>
/// <remarks>
/// <para>
/// A state machine is incremental by nature, so this — not a whole-string method — is the primitive;
/// every other entry point in the library is a few lines on top of it. That is why a string split
/// across chunk boundaries costs nothing to handle: there are no boundaries, only characters.
/// </para>
/// <para>
/// Line and column tracking lives here rather than in the machine. Where a character sits in a file
/// is a property of the file, not of the grammar, and keeping it out of the table is what lets the
/// table stay a pure description of the grammar.
/// </para>
/// <para>
/// Not thread safe. The machine it runs is, so any number of sessions may run concurrently.
/// </para>
/// </remarks>
public sealed class ScanSession
{
    private readonly CharClassifier _classifier;
    private readonly ScanContext _context;
    private Cursor<ScanState, CharClass, char, ScanContext> _cursor;
    private ScanResult? _result;
    private int _index;
    private int _line = 1;
    private int _column = 1;

    internal ScanSession(QuoteSyntax syntax)
    {
        _context = new ScanContext(syntax);
        _classifier = new CharClassifier(syntax.Quote, syntax.Escape);
        _cursor = ScannerMachine.Instance.CreateCursor();
    }

    /// <summary><see cref="Complete"/> has been called and the result is fixed.</summary>
    public bool IsCompleted => _result is not null;

    /// <summary>The input has already been found malformed; further characters are ignored.</summary>
    public bool IsFaulted => _context.Error is not null;

    /// <summary>Feeds the next run of characters.</summary>
    public void Feed(ReadOnlySpan<char> chunk)
    {
        ThrowIfCompleted();

        foreach (var character in chunk)
        {
            if (_cursor.IsTerminal)
            {
                return;
            }

            Consume(character);
        }
    }

    /// <summary>Feeds the next character.</summary>
    public void Feed(char character)
    {
        ThrowIfCompleted();

        if (!_cursor.IsTerminal)
        {
            Consume(character);
        }
    }

    /// <summary>
    /// Declares the input over and returns everything the scan found.
    /// </summary>
    /// <remarks>
    /// End of input is fed to the machine as a symbol in its own right, which is what turns
    /// "the text ran out inside a string" into a reported error rather than silence.
    /// Calling this more than once returns the same result.
    /// </remarks>
    public ScanResult Complete()
    {
        if (_result is not null)
        {
            return _result;
        }

        if (!_cursor.IsTerminal)
        {
            var endOfInput = '\0';
            _context.Position = new TextPosition(_index, _line, _column);
            _cursor.Advance(CharClass.EndOfInput, in endOfInput, _context);
        }

        _result = new ScanResult([.. _context.Segments], _context.Error);
        return _result;
    }

    private void Consume(char character)
    {
        _context.Position = new TextPosition(_index, _line, _column);
        _cursor.Advance(_classifier, in character, _context);

        _index++;

        if (character == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
    }

    private void ThrowIfCompleted()
    {
        if (_result is not null)
        {
            throw new InvalidOperationException("The session is complete and cannot accept more input.");
        }
    }
}
