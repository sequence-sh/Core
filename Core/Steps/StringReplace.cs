using System.Text;
using System.Text.RegularExpressions;

namespace Sequence.Core.Steps;

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
[AllowConstantFolding]
public sealed class StringReplace : CompoundStep<StringStream>, ICompoundStep
{
    /// <inheritdoc />
    protected override async ValueTask<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var inputResult = await String.WrapStringStream().Run(stateMonad, cancellationToken);

        if (inputResult.IsFailure)
            return inputResult.ConvertFailure<StringStream>();

        var input = inputResult.Value;

        Regex patternRegex;

        if (_compiledPatternRegex is not null)
        {
            patternRegex = _compiledPatternRegex;
        }
        else
        {
            var dataResult = await stateMonad.RunStepsAsync(
                Pattern.WrapStringStream(),
                IgnoreCase,
                cancellationToken
            );

            if (dataResult.IsFailure)
                return dataResult.ConvertFailure<StringStream>();

            var (pattern, ignoreCase) = dataResult.Value;

            var regexOptions = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            patternRegex = new Regex(pattern, regexOptions);
        }

        if (_simpleReplacementString is not null) //optimized happy path
        {
            var result = patternRegex.Replace(input, _simpleReplacementString);
            return new StringStream(result);
        }

        var replaceResult = await Replace.WrapNullable(StepMaps.String())
            .Run(stateMonad, cancellationToken);

        if (replaceResult.IsFailure)
            return replaceResult.ConvertFailure<StringStream>();

        var replace = replaceResult.Value;

        if (replace.HasNoValue && Function is null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.MissingParameter,
                nameof(Replace)
            );
        }

        if (replace.HasValue && Function is not null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(Replace),
                nameof(Function)
            );
        }

        var currentState =
            new Lazy<ImmutableDictionary<VariableName, ISCLObject>>(
                () => stateMonad.GetState().ToImmutableDictionary()
            );

        var sb        = new StringBuilder();
        var lastIndex = 0;

        foreach (Match match in patternRegex.Matches(input))
        {
            sb.Append(input, lastIndex, match.Index - lastIndex);

            string resultValue;

            if (Function is not null)
            {
                await using var scopedMonad = new ScopedStateMonad(
                    stateMonad,
                    currentState.Value,
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
    private static readonly Regex GroupsRegex = new("\\$(?<number>\\d)", RegexOptions.Compiled);

    private string? _simpleReplacementString;
    private Regex? _compiledPatternRegex;

    void ICompoundStep.ApplyOptimizations(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables)
    {
        var variableValues = new Lazy<IReadOnlyDictionary<VariableName, ISCLObject>>(
            () => injectedVariables.ToDictionary(x => x.Key, x => x.Value.SCLObject)
        );

        if (Replace is not null && Replace.HasConstantValue(injectedVariables.Keys))
        {
            var cvTask = Replace.TryGetConstantValueAsync(
                variableValues.Value,
                sfs
            );

            if (cvTask.IsCompleted)
            {
                var cvTaskString = cvTask.Result.Bind(x => x.MaybeAs<StringStream>())
                    .Map(x => x.GetString());

                if (cvTaskString.HasValue && !GroupsRegex.IsMatch(cvTaskString.Value))
                {
                    _simpleReplacementString = cvTaskString.Value;
                }
            }
        }

        if (Pattern.HasConstantValue(injectedVariables.Keys)
         && IgnoreCase.HasConstantValue(injectedVariables.Keys))
        {
            var patternTask = Pattern.TryGetConstantValueAsync(
                variableValues.Value,
                sfs
            );

            var ignoreCaseTask = IgnoreCase.TryGetConstantValueAsync(
                variableValues.Value,
                sfs
            );

            if (patternTask.IsCompleted && ignoreCaseTask.IsCompleted)
            {
                var ignoreCase = ignoreCaseTask.Result.Bind(x => x.MaybeAs<SCLBool>())
                    .Map(x => x.Value);

                if (ignoreCase.HasValue)
                {
                    var patternString = patternTask.Result.Bind(x => x.MaybeAs<StringStream>())
                        .Map(x => x.GetString());

                    if (patternString.HasValue)
                    {
                        var options = ignoreCase.Value
                            ? RegexOptions.IgnoreCase | RegexOptions.Compiled
                            : RegexOptions.None | RegexOptions.Compiled;

                        var patternRegex = new Regex(patternString.Value, options);
                        _compiledPatternRegex = patternRegex;
                    }
                }
            }
        }
    }

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
