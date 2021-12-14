namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Converts a string to an integer
/// </summary>
[Alias("ToInt")]
[SCLExample("StringToInt '123'", "123")]
public sealed class StringToInt : CompoundStep<SCLInt>
{
    /// <summary>
    /// The string to convert to an integer
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("String")]
    public IStep<StringStream> Integer { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<SCLInt, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Integer.WrapStringStream().Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return result.ConvertFailure<SCLInt>();

        if (int.TryParse(result.Value, out var i))
        {
            return i.ConvertToSCLObject();
        }

        return ErrorCode.CouldNotParse.ToErrorBuilder(result.Value, nameof(SCLInt))
            .WithLocationSingle(this);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringToInt, SCLInt>();
}
