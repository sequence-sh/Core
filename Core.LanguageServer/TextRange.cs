namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// A language server text range
/// </summary>
public class TextRange
{
    /// <summary>
    /// The line number of the start position
    /// </summary>
    public int StartLineNumber { get; set; }

    /// <summary>
    /// The column of the start position
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// The line number of the end position
    /// </summary>
    public int EndLineNumber { get; set; }

    /// <summary>
    /// The column of the end position
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// Create a new TextRange
    /// For Deserialization
    /// </summary>
    public TextRange() { }

    /// <summary>
    /// Create a new TextRange
    /// </summary>
    public TextRange(LinePosition start, LinePosition end)
    {
        StartLineNumber = start.Line;
        EndLineNumber   = end.Line;
        StartColumn     = start.Character;
        EndColumn       = end.Character;
    }

    /// <summary>
    /// Create a new TextRange
    /// </summary>
    public TextRange(int startLineNumber, int startColumn, int endLineNumber, int endColumn)
    {
        StartLineNumber = startLineNumber;
        StartColumn     = startColumn;
        EndLineNumber   = endLineNumber;
        EndColumn       = endColumn;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"[start: ({StartLineNumber}, {StartColumn}), end: ({EndLineNumber}, {EndColumn})]";
    }
}
