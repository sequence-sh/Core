namespace Sequence.Core.Steps;

/// <summary>
/// Returns a copy of the entity with the specified property removed
/// </summary>
[SCLExample(
    "EntityRemoveProperty Entity: ('type': 'A', 'value': 1) Property: 'type'",
    "('value': 1)"
)]
[SCLExample("Remove From: ('type': 'A', 'value': 1) Property: 'value'", "('type': \"A\")")]
[Alias("Remove")]
[Alias("RemoveProperty")]
[AllowConstantFolding]
public sealed class EntityRemoveProperty : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Entity, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var entity = await Entity.Run(stateMonad, cancellationToken);

        if (entity.IsFailure)
            return entity.ConvertFailure<Entity>();

        var property =
            await Property.Run(stateMonad, cancellationToken).Map(x => x.GetStringAsync());

        if (property.IsFailure)
            return property.ConvertFailure<Entity>();

        var newEntity = entity.Value.WithPropertyRemoved(new EntityKey(property.Value));

        return newEntity;
    }

    /// <summary>
    /// The entity to remove the property from
    /// </summary>
    [StepProperty(1)]
    [Required]
    [Alias("From")]
    public IStep<Entity> Entity { get; set; } = null!;

    /// <summary>
    /// The name of the property to remove
    /// </summary>
    [StepProperty]
    [Required]
    public IStep<StringStream> Property { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityRemoveProperty, Entity>();
}
