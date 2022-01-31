namespace Reductech.Sequence.Core.LanguageServer;

public static class QuickInfoHelper
{
    public static QuickInfoResponse GetQuickInfoAsync(
        string code,
        LinePosition position,
        StepFactoryStore stepFactoryStore)
    {
        var lazyTypeResolver = QuickInfoVisitor.CreateLazyTypeResolver(code, stepFactoryStore);
        var command          = Helpers.GetCommand(code, position);

        if (command is null)
            return new QuickInfoResponse();

        //We do not need the position offset because quick info doesn't return a position

        var visitor = new QuickInfoVisitor(
            command.Value.newPosition,
            stepFactoryStore,
            lazyTypeResolver
        );

        var errorListener = new ErrorErrorListener();

        var response = visitor.LexParseAndVisit(
            command.Value.command,
            x => { x.RemoveErrorListeners(); },
            x =>
            {
                x.RemoveErrorListeners();
                x.AddErrorListener(errorListener);
            }
        ) ?? new QuickInfoResponse();

        if (errorListener.Errors.Any())
        {
            var error = errorListener.Errors.First();

            return new QuickInfoResponse { MarkdownStrings = new List<string>() { error.Message } };
        }

        return response;
    }
}
