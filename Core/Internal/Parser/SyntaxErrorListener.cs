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

            foreach (var errorMatcher in ErrorMatchers)
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

            //Generic error case
            var error =
                ErrorCode.SCLSyntaxError.ToErrorBuilder(msg)
                    .WithLocation(new TextLocation(offendingSymbol));

            UncategorizedErrors.Add(error);
        }

        private static IEnumerable<IErrorMatcher> ErrorMatchers { get; } = new List<IErrorMatcher>()
        {
            OrderedAfterNamedArgumentErrorMatcher.Instance,
            UnclosedParenthesesErrorMatcher.Instance,
        };

        private interface IErrorMatcher
        {
            Maybe<IError> MatchError(
                RuleContext context,
                IToken offendingSymbol,
                Lazy<Maybe<FreezableStepProperty>> contextVisitResult);
        }

        private class UnclosedParenthesesErrorMatcher : IErrorMatcher
        {
            private UnclosedParenthesesErrorMatcher() { }
            public static IErrorMatcher Instance { get; } = new UnclosedParenthesesErrorMatcher();

            /// <inheritdoc />
            public Maybe<IError> MatchError(
                RuleContext context,
                IToken offendingSymbol,
                Lazy<Maybe<FreezableStepProperty>> contextVisitResult)
            {
                if (context is SCLParser.TermContext { Stop: null } termContext
                 && termContext.Start.Text == "(")
                {
                    var error =
                        ErrorCode.SCLSyntaxError.ToErrorBuilder("Unclosed Parentheses")
                            .WithLocation(new TextLocation(offendingSymbol));

                    return Maybe<IError>.From(error);
                }

                return Maybe<IError>.None;
            }
        }

        private class OrderedAfterNamedArgumentErrorMatcher : IErrorMatcher
        {
            private OrderedAfterNamedArgumentErrorMatcher() { }

            public static IErrorMatcher Instance { get; } =
                new OrderedAfterNamedArgumentErrorMatcher();

            /// <inheritdoc />
            public Maybe<IError> MatchError(
                RuleContext context,
                IToken offendingSymbol,
                Lazy<Maybe<FreezableStepProperty>> contextVisitResult)
            {
                if (!contextVisitResult.Value.HasValue)
                    return Maybe<IError>.None;

                if (contextVisitResult.Value.Value.MemberType != MemberType.Step
                 || contextVisitResult.Value.Value.ConvertToStep() is not CompoundFreezableStep cfs)
                    return Maybe<IError>.None;

                if (!cfs.FreezableStepData.StepProperties.Keys.Any(
                        x => x is StepParameterReference.Named
                    ))
                    return Maybe<IError>.None;

                var error =
                    ErrorCode.SCLSyntaxError.ToErrorBuilder(
                            "Ordered arguments cannot appear after Named Arguments"
                        )
                        .WithLocation(new TextLocation(offendingSymbol));

                return Maybe<IError>.From(error);
            }
        }
    }
}
