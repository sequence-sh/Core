namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Delay for a specified amount of time
/// </summary>
[Alias("Sleep")]
[SCLExample("Delay 10", description: "Delay 10 milliseconds.")]
public sealed class Delay : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async Task<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var ms = await Milliseconds.Run(stateMonad, cancellationToken);

        if (ms.IsFailure)
            return ms.ConvertFailure<Unit>();

        await Task.Delay(ms.Value.Value, cancellationToken);

        return Unit.Default;
    }

    /// <summary>
    /// The number of milliseconds to delay
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("ms")]
    public IStep<SCLInt> Milliseconds { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Delay, Unit>();
}
