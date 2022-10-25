namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Recursively flatten an entity, turning all nested entities into separate fields
/// </summary>
[SCLExample("(a.b.c: 1) | EntityFlatten", "('a.b.c': 1)")]
public sealed class EntityFlatten : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entityResult = await Entity.Run(stateMonad, cancellationToken);

        if (entityResult.IsFailure)
            return entityResult;

        var result = entityResult.Value.Flattened();

        return result;
    }

    /// <summary>
    /// The entity to set the property on.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => new SimpleStepFactory<EntityFlatten, Entity>();
}
