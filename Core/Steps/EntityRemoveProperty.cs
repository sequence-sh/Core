using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps;

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
public sealed class EntityRemoveProperty : CompoundStep<Entity>
{
    /// <inheritdoc />
    protected override async Task<Result<Entity, IError>> Run(
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

        var newEntity = entity.Value.RemoveProperty(property.Value);

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
