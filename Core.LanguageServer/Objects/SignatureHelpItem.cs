namespace Sequence.Core.LanguageServer.Objects;

/// <summary>
/// A language server signature help item
/// </summary>
public record SignatureHelpItem(
    string Name,
    string Label,
    string Documentation,
    IReadOnlyList<SignatureHelpParameter> Parameters);
