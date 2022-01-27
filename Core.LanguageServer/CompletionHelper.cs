namespace Reductech.Sequence.Core.LanguageServer;

public static class SignatureHelpHelper
{
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

/// <summary>
/// Gives code completion suggestions
/// </summary>
public static class CompletionHelper
{
    /// <summary>
    /// Gives a code completion suggestion
    /// </summary>
    public static CompletionResponse GetCompletionResponse(
        string code,
        LinePosition position,
        StepFactoryStore stepFactoryStore)
    {
        var visitor = new CompletionVisitor(position, stepFactoryStore);

        var completionList = visitor.LexParseAndVisit(
            code,
            x => { x.RemoveErrorListeners(); },
            x => { x.RemoveErrorListeners(); }
        ) ?? new CompletionResponse() { Items = new List<CompletionItem>() };

        return completionList;
    }
}
