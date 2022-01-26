using Reductech.Sequence.Core.Internal.Serialization;

namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Gets SCL Diagnostics
/// </summary>
public static class DiagnosticsHelper
{
    /// <summary>
    /// Get diagnostics for scl code
    /// </summary>
    public static IReadOnlyList<Diagnostic> GetDiagnostics(
        string text,
        StepFactoryStore stepFactoryStore)
    {
        List<Diagnostic> diagnostics;

        var initialParseResult = SCLParsing.TryParseStep(text);

        if (initialParseResult.IsSuccess)
        {
            var freezeResult = initialParseResult.Value.TryFreeze(
                SCLRunner.RootCallerMetadata,
                stepFactoryStore
            );

            if (freezeResult.IsSuccess)
            {
                diagnostics = new List<Diagnostic>();
            }

            else
            {
                diagnostics = freezeResult.Error.GetAllErrors()
                    .Select(x => ToDiagnostic(x, new LinePosition(0, 0)))
                    .WhereNotNull()
                    .ToList();
            }
        }
        else
        {
            var commands = Helpers.SplitIntoCommands(text);
            diagnostics = new List<Diagnostic>();

            foreach (var (commandText, commandPosition) in commands)
            {
                var visitor  = new DiagnosticsVisitor();
                var listener = new ErrorErrorListener();

                var parseResult = visitor.LexParseAndVisit(
                    commandText,
                    _ => { },
                    x => { x.AddErrorListener(listener); }
                );

                IList<Diagnostic> newDiagnostics = listener.Errors
                    .Select(x => ToDiagnostic(x, commandPosition))
                    .WhereNotNull()
                    .ToList();

                if (!newDiagnostics.Any())
                    newDiagnostics = parseResult.Select(x => ToDiagnostic(x, commandPosition))
                        .WhereNotNull()
                        .ToList();

                diagnostics.AddRange(newDiagnostics);
            }
        }

        return diagnostics;

        static Diagnostic? ToDiagnostic(SingleError error, LinePosition positionOffset)
        {
            if (error.Location.TextLocation is null)
                return null;

            var range = error.Location.TextLocation.GetRange(
                positionOffset.Line,
                positionOffset.Character
            );

            return new Diagnostic(
                new LinePosition(range.StartLineNumber, range.StartColumn),
                new LinePosition(range.EndLineNumber,   range.EndColumn),
                error.Message,
                8
            );
        }
    }
}
