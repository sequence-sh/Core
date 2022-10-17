namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Repeat a step a set number of times.
/// </summary>
[AllowConstantFolding]
[Alias("DoXTimes")]
[SCLExample("Repeat Action: (Log 1) Times: 3", ExpectedLogs = new[] { "1", "1", "1" })]
[SCLExample("DoXTimes (Log 1) X: 3",           ExpectedLogs = new[] { "1", "1", "1" })]
public sealed class Repeat : CompoundStep<Unit> //TODO replace with a lambda function
{
    /// <summary>
    /// The action to perform repeatedly.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Unit> Action { get; set; } = null!;

    /// <summary>
    /// The number of times to perform the action.
    /// </summary>
    [StepProperty(2)]
    [Required]
    [Alias("X")]
    public IStep<SCLInt> Times { get; set; } = null!;

    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var numberResult = await Times.Run(stateMonad, cancellationToken);

        if (numberResult.IsFailure)
            return numberResult.ConvertFailure<Unit>();

        for (var i = 0; i < numberResult.Value.Value; i++)
        {
            var result = await Action.Run(stateMonad, cancellationToken);

            if (result.IsFailure)
                return result.ConvertFailure<Unit>();
        }

        return Unit.Default;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<Repeat, Unit>();
}
