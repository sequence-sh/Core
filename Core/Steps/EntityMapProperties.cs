using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Change the name of entity fields.
/// </summary>
[Alias("RenameEntityFields")]
public class EntityMapProperties : CompoundStep<Array<Entity>>
{
    /// <inheritdoc />
    protected override async Task<Result<Array<Entity>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var mappings = await Mappings.Run(stateMonad, cancellationToken)
            .Map(
                e => e
                    .ToDictionary(x => x.Name, x => x.BestValue.ToString())
            );

        if (mappings.IsFailure)
            return mappings.ConvertFailure<Array<Entity>>();

        var entityStream = await EntityStream.Run(stateMonad, cancellationToken);

        if (entityStream.IsFailure)
            return entityStream.ConvertFailure<Array<Entity>>();

        var newEntityStream = entityStream.Value
            .Select(e => ChangeHeader(e, mappings.Value));

        return newEntityStream;

        static Entity ChangeHeader(Entity entity, IReadOnlyDictionary<string, string> mappings)
        {
            var builder = entity.Dictionary.ToBuilder();
            var changed = false;

            foreach (var property in entity)
            {
                if (mappings.TryGetValue(property.Name, out var newKey))
                {
                    builder.Remove(property.Name);

                    var newProperty = new EntityProperty(
                        newKey,
                        property.BaseValue,
                        property.NewValue,
                        property.Order
                    );

                    builder.Add(newKey, newProperty);
                    changed = true;
                }
            }

            if (!changed)
                return entity;

            return new Entity(builder.ToImmutable());
        }
    }

    /// <summary>
    /// The stream of entities to change the field names of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// An entity containing mappings
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Entity> Mappings { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityMapProperties, Array<Entity>>();
}

}
