using Antlr4.Runtime;

namespace Reductech.Sequence.Core.Internal.Parser;

/// <summary>
/// Matches SCL Syntax errors
/// </summary>
public interface IErrorMatcher
{
    /// <summary>
    /// Try to produce an appropriate error message for this context and symbol
    /// </summary>
    Maybe<SingleError> MatchError(
        RuleContext context,
        IToken offendingSymbol,
        Lazy<Maybe<FreezableStepProperty>> contextVisitResult);

    /// <summary>
    /// All existing error matchers
    /// </summary>
    public static IEnumerable<IErrorMatcher> All { get; } = new List<IErrorMatcher>()
    {
        OrderedAfterNamedArgumentErrorMatcher.Instance,
        UnclosedParenthesesErrorMatcher.Instance,
    };
}

/// <summary>
/// Matches unclosed parentheses
/// </summary>
public class UnclosedParenthesesErrorMatcher : IErrorMatcher
{
    private UnclosedParenthesesErrorMatcher() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IErrorMatcher Instance { get; } = new UnclosedParenthesesErrorMatcher();

    /// <inheritdoc />
    public Maybe<SingleError> MatchError(
        RuleContext context,
        IToken offendingSymbol,
        Lazy<Maybe<FreezableStepProperty>> contextVisitResult)
    {
        if (context is SCLParser.TermContext { Stop: null } termContext
         && termContext.Start.Text == "(")
        {
            var error =
                ErrorCode.SCLSyntaxError.ToErrorBuilder("Unclosed Parentheses")
                    .WithLocationSingle(new TextLocation(termContext));

            return Maybe<SingleError>.From(error);
        }

        if (offendingSymbol.Text == "(")
        {
            var error =
                ErrorCode.SCLSyntaxError.ToErrorBuilder("Unclosed Parentheses")
                    .WithLocationSingle(new TextLocation(offendingSymbol));

            return Maybe<SingleError>.From(error);
        }

        return Maybe<SingleError>.None;
    }
}

/// <summary>
/// Matches ordered arguments placed after named arguments
/// </summary>
public class OrderedAfterNamedArgumentErrorMatcher : IErrorMatcher
{
    private OrderedAfterNamedArgumentErrorMatcher() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IErrorMatcher Instance { get; } =
        new OrderedAfterNamedArgumentErrorMatcher();

    /// <inheritdoc />
    public Maybe<SingleError> MatchError(
        RuleContext context,
        IToken offendingSymbol,
        Lazy<Maybe<FreezableStepProperty>> contextVisitResult)
    {
        if (!contextVisitResult.Value.HasValue)
            return Maybe<SingleError>.None;

        if (contextVisitResult.Value.Value.MemberType != MemberType.Step
         || contextVisitResult.Value.Value.ConvertToStep() is not CompoundFreezableStep cfs)
            return Maybe<SingleError>.None;

        if (!cfs.FreezableStepData.StepProperties.Keys.Any(x => x is StepParameterReference.Named))
            return Maybe<SingleError>.None;

        var error =
            ErrorCode.SCLSyntaxError
                .ToErrorBuilder("Ordered arguments cannot appear after Named Arguments")
                .WithLocationSingle(new TextLocation(offendingSymbol));

        return Maybe<SingleError>.From(error);
    }
}
