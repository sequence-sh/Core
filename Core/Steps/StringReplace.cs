using System.Text;
using System.Text.RegularExpressions;

namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Replace every regex match in the string with the result of a particular function
/// </summary>
[SCLExample(
    "StringReplace String: 'number 1' Find: '1' Replace: 'one'",
    "number one",
    description: "Basic Replacement"
)]
[SCLExample(
    "StringReplace 'number 1' '\\w+' Function: (stringToCase <> TextCase.Upper)",
    "NUMBER 1",
    description: "Replace using a function"
)]
[SCLExample(
    "StringReplace 'number 13' '(\\w+)\\s+(\\d+)' '$2 was your $1'",
    "13 was your number",
    "Replace captured groups"
)]
[Alias("RegexReplace")]
public sealed class StringReplace : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var dataResult = await stateMonad.RunStepsAsync(
            String.WrapStringStream(),
            Pattern.WrapStringStream(),
            Replace.WrapNullable(StepMaps.String()),
            IgnoreCase,
            cancellationToken
        );

        if (dataResult.IsFailure)
            return dataResult.ConvertFailure<StringStream>();

        var (input, pattern, replace, ignoreCase) = dataResult.Value;

        if (replace.HasNoValue && Function is null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.MissingParameter,
                nameof(Replace)
            );
        }

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        var regexOptions = RegexOptions.None;

        if (ignoreCase.Value)
            regexOptions |= RegexOptions.IgnoreCase;

        var regex     = new Regex(pattern, regexOptions);
        var sb        = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in regex.Matches(input))
        {
            sb.Append(input, lastIndex, match.Index - lastIndex);

            string resultValue;

            if (Function is not null)
            {
                await using var scopedMonad = new ScopedStateMonad(
                    stateMonad,
                    currentState,
                    Function.VariableNameOrItem,
                    new KeyValuePair<VariableName, ISCLObject>(
                        Function.VariableNameOrItem,
                        new StringStream(match.Value)
                    )
                );

                var result = await Function.StepTyped.Run(scopedMonad, cancellationToken)
                    .Map(x => x.GetStringAsync());

                if (result.IsFailure)
                    return result.ConvertFailure<StringStream>();

                resultValue = result.Value;
            }

            else if (replace.HasValue)
            {
                var replacement = replace;

                resultValue =
                    GroupsRegex.Replace(
                        replacement.GetValueOrThrow(),
                        match1 =>
                        {
                            var num = int.Parse(match1.Groups["number"].Value);

                            if (match.Groups.Count > num)
                            {
                                var group = match.Groups[num];
                                return group.Value;
                            }

                            return match1.Value;
                        }
                    );
            }
            else
            {
                return new SingleError(
                    new ErrorLocation(this),
                    ErrorCode.MissingParameter,
                    nameof(Replace)
                );
            }

            sb.Append(resultValue);

            lastIndex = match.Index + match.Length;
        }

        sb.Append(input, lastIndex, input.Length - lastIndex);
        return new StringStream(sb.ToString());
    }

    /// <summary>
    /// The string to match
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("In")]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The regular expression pattern.
    /// Uses the .net flavor
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Find")]
    public IStep<StringStream> Pattern { get; set; } = null!;

    /// <summary>
    /// The replacement string.
    /// Use $1, $2 etc. to replace matched groups
    /// </summary>
    [StepProperty(3)]
    [DefaultValueExplanation("Either this or 'Function' must be set.")]
    public IStep<StringStream>? Replace { get; set; } = null!;

    /// <summary>
    /// A function to take the regex match and return the new string
    /// </summary>
    [FunctionProperty]
    [DefaultValueExplanation("Either this or 'Replace' must be set.")]
    public LambdaFunction<StringStream, StringStream>? Function { get; set; } = null!;

    /// <summary>
    /// Whether the regex should ignore case.
    /// </summary>
    [StepProperty()]
    [DefaultValueExplanation("False")]
    public IStep<SCLBool> IgnoreCase { get; set; } = new SCLConstant<SCLBool>(SCLBool.False);

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringReplace, StringStream>();

    /// <summary>
    /// Regex for matching regex groups
    /// </summary>
    private static readonly Regex GroupsRegex = new("\\$(?<number>\\d)");

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore)
    {
        if (Replace != null && Function != null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(Replace),
                nameof(Function)
            );
        }

        if (Replace is null && Function is null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.MissingParameter,
                nameof(Replace)
            );
        }

        return base.VerifyThis(stepFactoryStore);
    }
}
