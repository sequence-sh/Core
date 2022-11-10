namespace Sequence.Core.LanguageServer;

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
        StepFactoryStore sfs,
        DocumentationOptions documentationOptions)
    {
        var visitor = new SignatureHelpVisitor(linePosition, sfs, documentationOptions);

        var signatureHelpResponse = visitor.LexParseAndVisit(
            text,
            x => x.RemoveErrorListeners(),
            x => x.RemoveErrorListeners()
        );

        return signatureHelpResponse
            ?? new SignatureHelpResponse(0, 0, new List<SignatureHelpItem>());
    }
}
