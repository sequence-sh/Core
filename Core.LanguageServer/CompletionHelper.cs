namespace Reductech.Sequence.Core.LanguageServer;

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
        StepFactoryStore stepFactoryStore,
        DocumentationOptions documentationOptions,
        IReadOnlyDictionary<VariableName, InjectedVariable>? injectedVariables = null)
    {
        var lazyTypeResolver = new Lazy<TypeResolver>(
            () => SCLParsing.CreateTypeResolver(
                code,
                stepFactoryStore,
                injectedVariables
            )
        );

        var visitor = new CompletionVisitor(
            position,
            stepFactoryStore,
            lazyTypeResolver,
            documentationOptions
        );

        var completionResponse = visitor.LexParseAndVisit(
            code,
            x => { x.RemoveErrorListeners(); },
            x => { x.RemoveErrorListeners(); }
        );

        if (completionResponse is not null)
            return completionResponse;

        var command = Helpers.GetCommand(code, position);

        if (command is not null)
        {
            visitor = new CompletionVisitor(
                command.Value.newPosition,
                stepFactoryStore,
                lazyTypeResolver,
                documentationOptions
            );

            var lineCompletionResponse = visitor.LexParseAndVisit(
                command.Value.command,
                x => { x.RemoveErrorListeners(); },
                x => { x.RemoveErrorListeners(); }
            );

            if (lineCompletionResponse is not null)
                return lineCompletionResponse.Offset(command.Value.positionOffset);

            var (newText, removedToken) = Helpers.RemoveToken(
                command.Value.command,
                command.Value.newPosition
            );

            var withoutTokenResponse = visitor.LexParseAndVisit(
                newText,
                x => { x.RemoveErrorListeners(); },
                x => { x.RemoveErrorListeners(); }
            );

            if (withoutTokenResponse is not null)
                return withoutTokenResponse.Offset(
                    new LinePosition(
                        command.Value.positionOffset.Line,
                        -removedToken?.Text.Length ?? 0
                    )
                );
        }

        return new CompletionResponse(true, ArraySegment<CompletionItem>.Empty);
    }
}
