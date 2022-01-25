namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class Request : SimpleFileRequest
{
    public int Line { get; set; }

    public int Column { get; set; }

    public string Buffer { get; set; }

    public IEnumerable<LinePositionSpanTextChange> Changes { get; set; }

    public bool ApplyChangesTogether { get; set; }
}
