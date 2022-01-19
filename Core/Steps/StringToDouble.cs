namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Converts a string to a double
/// </summary>
[Alias("ToDouble")]
[SCLExample("StringToDouble '123.45'", "123.45")]
public sealed class StringToDouble : CompoundStep<SCLDouble>
{
    /// <summary>
    /// The string to convert to an integer
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("String")]
    public IStep<StringStream> Double { get; set; } = null!;

    /// <inheritdoc />
    protected override async Task<Result<SCLDouble, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Double.WrapStringStream().Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return result.ConvertFailure<SCLDouble>();

        if (double.TryParse(result.Value, out var d))
        {
            return d.ConvertToSCLObject();
        }

        return ErrorCode.CouldNotParse.ToErrorBuilder(result.Value, nameof(SCLDouble))
            .WithLocationSingle(this);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<StringToDouble, SCLDouble>();
}
