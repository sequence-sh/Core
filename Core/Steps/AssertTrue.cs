namespace Sequence.Core.Steps;

/// <summary>
/// Returns an error if the nested step does not return true.
/// </summary>
[SCLExample("AssertTrue ((2 + 2) == 4)")]
public sealed class AssertTrue : CompoundStep<Unit>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Boolean.Run(stateMonad, cancellationToken)
            .Ensure(
                x => x.Value,
                new SingleError(
                    new ErrorLocation(this),
                    ErrorCode.AssertionFailed,
                    Boolean.Name
                )
            )
            .Map(_ => Unit.Default);
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<AssertTrue, Unit>();

    /// <summary>
    /// The bool to test.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<SCLBool> Boolean { get; set; } = null!;
}
