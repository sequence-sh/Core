namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Get an entity containing SCL settings
/// </summary>
public sealed class GetSettings : CompoundStep<Entity>
{
    /// <inheritdoc />
    #pragma warning disable CS1998
    protected override async ValueTask<Result<Entity, IError>> Run(
        #pragma warning restore CS1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = stateMonad.Settings;

        return entity;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetSettings, Entity>();
}
