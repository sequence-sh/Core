namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Returns whether a string is empty.
/// </summary>
[Alias("IsStringEmpty")]
public sealed class StringIsEmpty : CompoundStep<SCLBool>
{
    /// <inheritdoc />
    protected override async Task<Result<SCLBool, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var str = await String.Run(stateMonad, cancellationToken)
            .Map(async x => await x.GetStringAsync());

        if (str.IsFailure)
            return str.ConvertFailure<SCLBool>();

        return string.IsNullOrWhiteSpace(str.Value).ConvertToSCLObject();
    }

    /// <summary>
    /// The string to check for being empty.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<StringStream> String { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringIsEmpty, SCLBool>();
}
