using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema;

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
                        r.Value.GetValueOrThrow(),
                        null
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
