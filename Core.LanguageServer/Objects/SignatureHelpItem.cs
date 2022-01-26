namespace Reductech.Sequence.Core.LanguageServer.Objects;

public record SignatureHelpItem(
    string Name,
    string Label,
    string Documentation,
    IReadOnlyList<SignatureHelpParameter> Parameters);
