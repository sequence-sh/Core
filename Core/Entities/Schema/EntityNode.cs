using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Generator.Equals;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// A schema which, if matched, allows additional items
/// </summary>
public record EntityAdditionalItems(SchemaNode AdditionalItems) : NodeData<EntityAdditionalItems>
{
    /// <inheritdoc />
    public override EntityAdditionalItems Combine(EntityAdditionalItems other)
    {
        return new EntityAdditionalItems(AdditionalItems.Combine(other.AdditionalItems));
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        builder.AdditionalItems(AdditionalItems.ToJsonSchema());
    }
}

/// <summary>
/// Data about entity properties
/// </summary>
[Equatable]
public partial record EntityPropertiesData(
    [property: SetEquality] IReadOnlyDictionary<string, (SchemaNode Node, bool Required)> Nodes) :
    NodeData<
        EntityPropertiesData>
{
    /// <inheritdoc />
    public override EntityPropertiesData Combine(EntityPropertiesData other)
    {
        if (this == Empty)
            return other;

        if (other == Empty)
            return this;

        var newDict = new Dictionary<string, (SchemaNode Node, bool Required)>();

        foreach (var group in Nodes.Concat(other.Nodes)
            .GroupBy(x => x.Key, x => x.Value))
        {
            var key = group.Key;

            if (group.Count() == 1)
                newDict[key] = group.Single();

            var (first, second) = group.GetFirstTwo().GetValueOrThrow();

            newDict[key] = (first.Node.Combine(second.Node), first.Required || second.Required);
        }

        return new EntityPropertiesData(newDict);
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        var requiredStuff = Nodes
            .Where(x => x.Value.Required)
            .Select(x => x.Key)
            .ToList();

        if (requiredStuff.Any())
            builder.Required(requiredStuff);

        var props = Nodes.ToDictionary(x => x.Key, x => x.Value.Node.ToJsonSchema());

        if (props.Any())
            builder.Properties(props);
    }

    /// <summary>
    /// EntityPropertiesData with no Properties
    /// </summary>
    public static EntityPropertiesData Empty { get; } = new(
        ImmutableDictionary<string, (SchemaNode Node, bool Required)>.Empty
    );
}

/// <summary>
/// Matches a structured object
/// </summary>
public record EntityNode(
        EnumeratedValuesNodeData EnumeratedValuesNodeData,
        EntityAdditionalItems EntityAdditionalItems,
        EntityPropertiesData EntityPropertiesData)
    : SchemaNode<EntityAdditionalItems, EntityPropertiesData>(
        EnumeratedValuesNodeData,
        EntityAdditionalItems,
        EntityPropertiesData
    )
{
    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Object;

    /// <inheritdoc />
    public override bool IsMorePermissive(SchemaNode other) => false;

    /// <inheritdoc />
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        if (entityValue is not EntityValue.NestedEntity nestedEntity)
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should be Entity", propertyName);

        var errors = new List<IErrorBuilder>();

        var remainingRequiredProperties =
            EntityPropertiesData.Nodes.Where(x => x.Value.Required)
                .Select(x => x.Key)
                .ToHashSet();

        var newEntity = nestedEntity.Value;
        var changed   = false;

        bool allowExtra;

        if (transformSettings.RemoveExtra.HasValue)
            allowExtra = transformSettings.RemoveExtra.GetValueOrThrow();
        else
        {
            allowExtra = EntityAdditionalItems.AdditionalItems
                .TryTransform(propertyName, entityValue, transformSettings)
                .IsSuccess;
        }

        foreach (var entityProperty in nestedEntity.Value)
        {
            if (EntityPropertiesData.Nodes.TryGetValue(entityProperty.Name, out var node))
            {
                var r = node.Node.TryTransform(
                    propertyName + "." + entityProperty.Name,
                    entityProperty.Value,
                    transformSettings
                );

                if (r.IsFailure)
                    errors.Add(r.Error);
                else if (r.Value.HasValue)
                {
                    newEntity = newEntity.WithProperty(
                        entityProperty.Name,
                        r.Value.GetValueOrThrow()
                    );

                    changed = true;
                }
            }
            else if (allowExtra)
            {
                newEntity = newEntity.RemoveProperty(entityProperty.Name);
                changed   = true;
            }

            remainingRequiredProperties.Remove(entityProperty.Name);
        }

        foreach (var remainingRequiredProperty in remainingRequiredProperties)
        {
            errors.Add(
                ErrorCode.SchemaViolation.ToErrorBuilder(
                    $"Missing Property '{remainingRequiredProperty}'",
                    propertyName
                )
            );
        }

        if (errors.Any())
            return Result.Failure<Maybe<EntityValue>, IErrorBuilder>(
                ErrorBuilderList.Combine(errors)
            );

        if (changed)
            return Maybe<EntityValue>.From(new EntityValue.NestedEntity(newEntity));

        return Maybe<EntityValue>.None;
    }
}

}
