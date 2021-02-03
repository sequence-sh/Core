using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Replace every regex match in the string with the result of a particular function
/// </summary>
public sealed class RegexReplace : CompoundStep<string>
{
    /// <inheritdoc />
    protected override async Task<Result<string, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult =
            await String.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<string>();

        var patternResult =
            await Pattern.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (patternResult.IsFailure)
            return patternResult.ConvertFailure<string>();

        var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

        if (ignoreCaseResult.IsFailure)
            return ignoreCaseResult.ConvertFailure<string>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        var regexOptions = RegexOptions.None;

        if (ignoreCaseResult.Value)
            regexOptions |= RegexOptions.IgnoreCase;

        var regex     = new Regex(patternResult.Value, regexOptions);
        var input     = stringResult.Value;
        var sb        = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in regex.Matches(input))
        {
            sb.Append(input, lastIndex, match.Index - lastIndex);

            using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                new KeyValuePair<VariableName, object>(Variable, match.Value)
            );

            var result = await Function.Run(scopedMonad, cancellationToken)
                .Map(x => x.GetStringAsync());

            if (result.IsFailure)
                return result.ConvertFailure<string>();

            sb.Append(result.Value);

            lastIndex = match.Index + match.Length;
        }

        sb.Append(input, lastIndex, input.Length - lastIndex);
        return sb.ToString();
    }

    /// <summary>
    /// The string to match
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The regular expression pattern.
    /// Uses the .net flavor
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> Pattern { get; set; } = null!;

    /// <summary>
    /// Whether the regex should ignore case.
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("False")]
    public IStep<bool> IgnoreCase { get; set; } = new BoolConstant(false);

    /// <summary>
    /// A function to take the regex match and return the new string
    /// </summary>
    [StepProperty(2)]
    [Required]
    [ScopedFunction]
    public IStep<StringStream> Function { get; set; } = null!;

    /// <summary>
    /// The variable name to use for the match in the function.
    /// </summary>
    [VariableName(3)]
    [DefaultValueExplanation("<Match>")]
    public VariableName Variable { get; set; } = VariableName.Match;

    /// <inheritdoc />
    public override Result<StepContext, IError> TryGetScopedContext(
        StepContext baseContext,
        IFreezableStep scopedStep)
    {
        return baseContext.TryCloneWithScopedStep(
            Variable,
            new ActualTypeReference(typeof(string)),
            scopedStep,
            new StepErrorLocation(this)
        );
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => RegexReplaceStepFactory.Instance;
}

/// <summary>
/// Replace every regex match in the string with the result of a particular function
/// </summary>
public sealed class RegexReplaceStepFactory : SimpleStepFactory<RegexReplace, string>
{
    private RegexReplaceStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SimpleStepFactory<RegexReplace, string> Instance { get; } =
        new RegexReplaceStepFactory();
}

}
