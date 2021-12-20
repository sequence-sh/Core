namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Join strings with a delimiter.
/// </summary>
[Alias("JoinStrings")]
public sealed class StringJoin : CompoundStep<StringStream>
{
    /// <summary>
    /// The string to join.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<StringStream>> Strings { get; set; } = null!;

    /// <summary>
    /// The delimiter to use.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("Empty String")]
    [Alias("Using")]
    public IStep<StringStream> Delimiter { get; set; } = new SCLConstant<StringStream>("");

    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var listResult = await Strings.Run(stateMonad, cancellationToken)
            .Bind(x => x.GetElementsAsync(cancellationToken));

        if (listResult.IsFailure)
            return listResult.ConvertFailure<StringStream>();

        var delimiter = await Delimiter.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (delimiter.IsFailure)
            return delimiter.ConvertFailure<StringStream>();

        if (listResult.Value.Count == 0)
            return StringStream.Empty;

        var strings = new List<string>();

        foreach (var ss in listResult.Value)
            strings.Add(await ss.GetStringAsync());

        var resultString = string.Join(delimiter.Value, strings);

        return new StringStream(resultString);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new
        SimpleStepFactory<StringJoin, StringStream>();
}
