using System;
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
using Reductech.EDR.Core.Util;

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
        var r = await stateMonad.RunStepsAsync(EntityStream, Mappings, cancellationToken);

        if (r.IsFailure)
            return r.ConvertFailure<Array<Entity>>();

        var (entityStream, mappings) = r.Value;

        var mappingsDict = mappings.ToDictionary(
            x => x.Name,
            x => GetStringList(x.Value),
            StringComparer.OrdinalIgnoreCase
        );

        var propertiesToRemove = mappingsDict
            .SelectMany(x => x.Value)
            .ToHashSet();

        var newEntityStream = entityStream
            .Select(e => ChangePropertyNames(e, mappingsDict, propertiesToRemove));

        return newEntityStream;

        static Entity ChangePropertyNames(
            Entity entity,
            IReadOnlyDictionary<string, IReadOnlyList<EntityPropertyKey>> mappings,
            IEnumerable<EntityPropertyKey> propertiesToRemove)
        {
            var changed = false;

            var newEntity = entity;

            foreach (var (newName, propertyKeys) in mappings)
            foreach (var entityPropertyKey in propertyKeys)
            {
                var value = entity.TryGetProperty(entityPropertyKey);

                if (value.HasValue)
                {
                    changed = true;
                    var newProperty = value.GetValueOrThrow();

                    newEntity = newEntity.WithProperty(
                        newName,
                        newProperty.Value,
                        newProperty.Order
                    );

                    break;
                }
            }

            var withoutProperties = newEntity.TryRemoveProperties(propertiesToRemove);

            if (withoutProperties.HasValue)
            {
                newEntity = withoutProperties.GetValueOrThrow();
                changed   = true;
            }

            if (!changed)
                return entity;

            return newEntity;
        }

        static IReadOnlyList<EntityPropertyKey> GetStringList(EntityValue ev)
        {
            if (ev is EntityValue.NestedList nestedList)
            {
                return nestedList.Value
                    .Select(x => EntityPropertyKey.Create(x.GetPrimitiveString()))
                    .ToList();
            }

            return new[] { EntityPropertyKey.Create(ev.GetPrimitiveString()) };
        }
    }

    /// <summary>
    /// The stream of entities to change the field names of.
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;

    /// <summary>
    /// An entity containing mappings.
    /// The keys in the entity will be the new column names.
    /// The value of each property should either the the name of the source column or a list of source column names
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<Entity> Mappings { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory { get; } =
        new SimpleStepFactory<EntityMapProperties, Array<Entity>>();
}

}
