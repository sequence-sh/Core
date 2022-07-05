using System.IO;
using Antlr4.Runtime;

namespace Reductech.Sequence.Core.Internal.Parser;

public static partial class SCLParsing
{
    /// <summary>
    /// Error listener that looks for syntax errors
    /// </summary>
    public class SyntaxErrorListener : BaseErrorListener
    {
        /// <summary>
        /// Errors found by this error listener.
        /// </summary>
        public readonly List<IError> Errors = new();

        public readonly List<IError> UncategorizedErrors = new();

        /// <inheritdoc />
        public override void SyntaxError(
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e)
        {
            if (e?.Context is not null)
            {
                var lazyVisitResult =
                    new Lazy<Maybe<FreezableStepProperty>>(
                        () => new Visitor().Visit(e.Context)
                            .ToMaybe()
                            .Bind(
                                x => x == null
                                    ? Maybe<FreezableStepProperty>.None
                                    : Maybe<FreezableStepProperty>.From(x)
                            )
                    );

                foreach (var errorMatcher in IErrorMatcher.All)
                {
                    var maybeError = errorMatcher.MatchError(
                        e.Context,
                        offendingSymbol,
                        lazyVisitResult
                    );

                    if (maybeError.HasValue)
                    {
                        Errors.Add(maybeError.Value);
                        return;
                    }
                }
            }

            //Generic error case
            var error =
                ErrorCode.SCLSyntaxError.ToErrorBuilder(msg)
                    .WithLocation(new TextLocation(offendingSymbol));

            UncategorizedErrors.Add(error);
        }
    }
}
