namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record Diagnostic(LinePosition Start, LinePosition End, string Message, int Severity);
