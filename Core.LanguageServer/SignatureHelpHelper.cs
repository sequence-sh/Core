namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Gets signature help
/// </summary>
public static class SignatureHelpHelper
{
    /// <summary>
    /// Gets signature help for a particular position
    /// </summary>
    public static SignatureHelpResponse GetSignatureHelpResponse(
        string text,
        LinePosition linePosition,
        StepFactoryStore sfs)
    {
        var visitor = new SignatureHelpVisitor(linePosition, sfs);

        var signatureHelpResponse = visitor.LexParseAndVisit(
            text,
            x => x.RemoveErrorListeners(),
            x => x.RemoveErrorListeners()
        );

        return signatureHelpResponse
            ?? new SignatureHelpResponse(0, 0, new List<SignatureHelpItem>());
    }
}
