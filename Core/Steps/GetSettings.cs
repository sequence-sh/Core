namespace Reductech.EDR.Core.Steps;

/// <summary>
/// Get an entity containing SCL settings
/// </summary>
public sealed class GetSettings : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        var entity = stateMonad.Settings;

        return entity;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<GetSettings, Entity>();
}
