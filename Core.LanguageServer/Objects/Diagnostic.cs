namespace Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A language server diagnostic
/// </summary>
public record Diagnostic(LinePosition Start, LinePosition End, string Message, int Severity);
