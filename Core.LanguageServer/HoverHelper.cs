namespace Reductech.Sequence.Core.LanguageServer;

public static class HoverHelper
{
    public static QuickInfoResponse GetQuickInfoAsync(
        string code,
        QuickInfoRequest quickInfoRequest,
        StepFactoryStore stepFactoryStore)
    {
        var lazyTypeResolver = HoverVisitor.CreateLazyTypeResolver(code, stepFactoryStore);
        var position         = new LinePosition(quickInfoRequest.Line, quickInfoRequest.Column);
        var command          = Helpers.GetCommand(code, position);

        if (command is null)
            return new QuickInfoResponse();

        var visitor2 = new HoverVisitor(
            command.Value.newPosition,
            command.Value.positionOffset,
            stepFactoryStore,
            lazyTypeResolver
        );

        var errorListener = new ErrorErrorListener();

        var response = visitor2.LexParseAndVisit(
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
