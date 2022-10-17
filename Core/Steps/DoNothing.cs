namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Does nothing.
/// </summary>
[SCLExample("DoNothing", description: "Does nothing")]
[AllowConstantFolding]
public class DoNothing : CompoundStep<Unit>
{
    /// <inheritdoc />
    #pragma warning disable CS1998
    protected override async ValueTask<Result<Unit, IError>> Run(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Unit.Default;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } = new SimpleStepFactory<DoNothing, Unit>();
}
