namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Splits a string.
/// </summary>
[Alias("SplitString")]
public sealed class StringSplit : CompoundStep<Array<StringStream>>
{
    /// <summary>
    /// The string to split.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <summary>
    /// The delimiter to use.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("Using")]
    public IStep<StringStream> Delimiter { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<Array<StringStream>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var stringResult = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (stringResult.IsFailure)
            return stringResult.ConvertFailure<Array<StringStream>>();

        var delimiterResult = await Delimiter.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (delimiterResult.IsFailure)
            return delimiterResult.ConvertFailure<Array<StringStream>>();

        var results = stringResult.Value
            .Split(new[] { delimiterResult.Value }, StringSplitOptions.None)
            .Select(x => new StringStream(x) as StringStream)
            .ToList()
            .ToSCLArray();

        return results;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringSplit, Array<StringStream>>();
}
