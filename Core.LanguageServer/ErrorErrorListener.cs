namespace Reductech.Sequence.Core.LanguageServer;

/// <summary>
/// Listens for errors in the SCL
/// </summary>
public class ErrorErrorListener : IAntlrErrorListener<IToken>
{
    /// <summary>
    /// The errors which have been found
    /// </summary>
    public List<SingleError> Errors = new();

    /// <inheritdoc />
    public void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        if (e is not null)
        {
            var lazyVisitResult =
                new Lazy<Maybe<FreezableStepProperty>>(
                    () => new SCLParsing.Visitor().Visit(e.Context)
                        .ToMaybe()
                        .Bind(
                            x => x == null
                                ? Maybe<FreezableStepProperty>.None
                                : Maybe<FreezableStepProperty>.From(x)
                        )
                );

            foreach (var errorMatcher in IErrorMatcher.All)
            {
                var specificError = errorMatcher.MatchError(
                    e.Context,
                    offendingSymbol,
                    lazyVisitResult
                );

                if (specificError.HasValue)
                {
                    Errors.Add(specificError.Value);
                    return;
                }
            }
        }

        TextLocation textLocation;

        if (e is NoViableAltException noViableAltException)
        {
            textLocation = new TextLocation(
                noViableAltException.StartToken.Text, //not the correct text
                new TextPosition(noViableAltException.StartToken),
                new TextPosition(offendingSymbol)
            );
        }
        else
        {
            textLocation = new TextLocation(offendingSymbol);
        }

        var errorBuilder = ErrorCode.SCLSyntaxError.ToErrorBuilder(msg);
        var error        = errorBuilder.WithLocationSingle(textLocation);
        Errors.Add(error);
    }
}
