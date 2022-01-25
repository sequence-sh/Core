namespace Reductech.Sequence.Core.LanguageServer;

public class TextRange
{
    public int StartLineNumber { get; set; }

    public int StartColumn { get; set; }

    public int EndLineNumber { get; set; }

    public int EndColumn { get; set; }

    public TextRange() { }

    public TextRange(LinePosition start, LinePosition end)
    {
        StartLineNumber = start.Line;
        EndLineNumber   = end.Line;
        StartColumn     = start.Character;
        EndColumn       = end.Character;
    }

    public TextRange(int startLineNumber, int startColumn, int endLineNumber, int endColumn)
    {
        StartLineNumber = startLineNumber;
        StartColumn     = startColumn;
        EndLineNumber   = endLineNumber;
        EndColumn       = endColumn;
    }
}
