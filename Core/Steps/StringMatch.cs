using System.Text.RegularExpressions;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Returns true if a string is matched by a particular regular expression
/// </summary>
[SCLExample("StringMatch String: 'aaaabbbbccc' Pattern: 'a+b+c+'", "True")]
[SCLExample("IsMatch String: 'abracadabra' Regex: 'ab\\w+?ab'",    "True")]
[Alias("IsMatch")]
[Alias("RegexMatch")]
public sealed class StringMatch : CompoundStep<SCLBool>
{
    /// <inheritdoc />
    protected override async Task<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult =
            await String.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<SCLBool>();

        var patternResult =
            await Pattern.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (patternResult.IsFailure)
            return patternResult.ConvertFailure<SCLBool>();

        var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

        if (ignoreCaseResult.IsFailure)
            return ignoreCaseResult.ConvertFailure<SCLBool>();

        var regexOptions = RegexOptions.None;

        if (ignoreCaseResult.Value.Value)
            regexOptions |= RegexOptions.IgnoreCase;

        var isMatch = Regex.IsMatch(stringResult.Value, patternResult.Value, regexOptions);

        return isMatch.ConvertToSCLObject();
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
    [Alias("Regex")]
    public IStep<StringStream> Pattern { get; set; } = null!;

    /// <summary>
    /// Whether the regex should ignore case.
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("False")]
    public IStep<SCLBool> IgnoreCase { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringMatch, SCLBool>();
}
