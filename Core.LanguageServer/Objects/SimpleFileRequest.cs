namespace Reductech.Sequence.Core.LanguageServer.Objects;

public class SimpleFileRequest
{
    private string? _fileName;

    public string FileName
    {
        get => _fileName?.Replace(
            Path.AltDirectorySeparatorChar,
            Path.DirectorySeparatorChar
        ) ?? "";
        set => _fileName = value;
    }
}
