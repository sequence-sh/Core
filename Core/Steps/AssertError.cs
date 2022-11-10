namespace Sequence.Core.Steps;

/// <summary>
/// Returns success if the ValueIf step returns an error and a failure otherwise.
/// </summary>
[SCLExample("AssertError (log (1 / 0))")]
public sealed class AssertError : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Step.Run(stateMonad, cancellationToken);

        if (result.IsFailure)
            return Unit.Default;

        return new SingleError(
            new ErrorLocation(this),
            ErrorCode.AssertionFailed,
            Step.Name
        );
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<AssertError, Unit>();

    /// <summary>
    /// The step to test.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Unit> Step { get; set; } = null!;
}
