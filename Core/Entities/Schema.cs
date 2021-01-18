using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// An entity schema.
/// Enforces that the entity matches certain constraints.
/// </summary>
public sealed class Schema
{
    /// <summary>
    /// The name of the schema.
    /// </summary>
    [ConfigProperty(1)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// The schema properties.
    /// </summary>
    [ConfigProperty(2)]
    public Dictionary<string, SchemaProperty> Properties { get; set; } =
        null!; //public setter for deserialization

    /// <summary>
    /// Whether properties other than the explicitly defined properties are allowed.
    /// </summary>
    [ConfigProperty(3)]
    public bool AllowExtraProperties { get; set; } = true;

    /// <inheritdoc />
    public override string ToString()
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        return Name ?? "Schema";
    }

    /// <summary>
    /// Attempts to apply this schema to an entity.
    /// </summary>
    public Result<Entity, IErrorBuilder> ApplyToEntity(Entity entity)
    {
        var remainingProperties = Properties
            .ToDictionary(
                x => x.Key,
                x => x.Value,
                StringComparer.OrdinalIgnoreCase
            );

        var newProperties = new List<EntityProperty>();
        var errors        = new List<IErrorBuilder>();
        var changed       = false;

        foreach (var entityProperty in entity)
        {
            if (remainingProperties.Remove(entityProperty.Name, out var schemaProperty))
            {
                var convertResult = entityProperty.BestValue.TryConvert(schemaProperty);

                if (convertResult.IsSuccess)
                {
                    if (convertResult.Value.changed)
                    {
                        changed = true;

                        var newProperty = new EntityProperty(
                            entityProperty.Name,
                            entityProperty.BaseValue,
                            convertResult.Value.value,
                            entityProperty.Order
                        );

                        newProperties.Add(newProperty);
                    }
                    else
                    {
                        newProperties.Add(entityProperty);
                    }
                }

                else
                    errors.Add(convertResult.Error);
            }
            else if (AllowExtraProperties) //This entity has a property that is not in the schema
                newProperties.Add(entityProperty);
            else
                errors.Add(
                    new ErrorBuilder(
                        ErrorCode.SchemaViolationUnexpectedProperty,
                        entityProperty.Name
                    )
                );
        }

        foreach (var (key, _) in remainingProperties
            .Where(
                x => x.Value.Multiplicity == Multiplicity.ExactlyOne
                  || x.Value.Multiplicity == Multiplicity.AtLeastOne
            ))
            errors.Add(new ErrorBuilder(ErrorCode.SchemaViolationMissingProperty, key));

        if (errors.Any())
        {
            var l = ErrorBuilderList.Combine(errors);
            return Result.Failure<Entity, IErrorBuilder>(l);
        }

        if (!changed)
            return entity;

        var resultEntity = new Entity(newProperties);

        return resultEntity;
    }

    /// <summary>
    /// Tries to create a schema from an entity.
    /// Ignores unexpected properties.
    /// </summary>
    public static Result<Schema, IErrorBuilder> TryCreateFromEntity(Entity entity)
    {
        var results = new List<Result<Unit, IErrorBuilder>>();
        var schema  = new Schema();

        results.Add(entity.TrySetString(nameof(Name), nameof(Schema), s => schema.Name = s));

        results.Add(
            entity.TrySetBoolean(
                nameof(AllowExtraProperties),
                nameof(Schema),
                s => schema.AllowExtraProperties = s
            )
        );

        results.Add(
            entity.TrySetDictionary(
                nameof(Properties),
                nameof(Schema),
                ev =>
                {
                    var childEntity = ev.TryGetEntity();

                    if (childEntity.HasValue)
                        return SchemaProperty.TryCreateFromEntity(childEntity.Value);

                    return new ErrorBuilder(
                        ErrorCode.InvalidCast,
                        ev,
                        typeof(SchemaProperty).Name
                    );
                },
                d => schema.Properties = d
            )
        );

        var r = results.Combine(ErrorBuilderList.Combine)
            .Map(_ => schema);

        return r;
    }

    /// <summary>
    /// Converts a schema to an entity for deserialization
    /// </summary>
    public Entity ConvertToEntity()
    {
        var propertiesEntity =
            new Entity(
                Properties.Select(
                    (x, i) =>
                        new EntityProperty(
                            x.Key,
                            new EntityValue(x.Value.ConvertToEntity()),
                            null,
                            i
                        )
                )
            );

        var topProperties = new[]
        {
            (nameof(Name), EntityValue.CreateFromObject(Name)),
            (nameof(AllowExtraProperties), EntityValue.CreateFromObject(AllowExtraProperties)),
            (nameof(Properties), EntityValue.CreateFromObject(propertiesEntity)),
        }.Select((x, i) => new EntityProperty(x.Item1, x.Item2, null, i));

        var entity = new Entity(topProperties);

        return entity;
    }
}

}
