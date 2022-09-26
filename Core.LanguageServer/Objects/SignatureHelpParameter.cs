namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A language server signature help parameter
/// </summary>
public record SignatureHelpParameter(string Name, string Label, string Documentation);
