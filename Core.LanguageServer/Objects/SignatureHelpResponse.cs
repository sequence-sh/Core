namespace Reductech.Sequence.Core.LanguageServer.Objects;

/// <summary>
/// The response of a signature help request
/// </summary>
public record SignatureHelpResponse(
    int ActiveSignature,
    int ActiveParameter,
    IReadOnlyList<SignatureHelpItem> Signatures)
{
    /// <summary>
    /// Empty response
    /// </summary>
    public static SignatureHelpResponse Empty { get; } =
        new(0, 0, new List<SignatureHelpItem>());
}
