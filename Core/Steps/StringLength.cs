namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Calculates the length of the string.
/// </summary>
[Alias("StrLen")]
[AllowConstantFolding]
public sealed class StringLength : CompoundStep<SCLInt>
{
    /// <summary>
    /// The string to measure the length of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("Of")]
    public IStep<StringStream> String { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<SCLInt, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<SCLInt>();

        return str.Value.Length.ConvertToSCLObject();
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringLength, SCLInt>();
}
