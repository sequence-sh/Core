namespace Reductech.Sequence.Core.Entities.Schema;

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
    public override bool IsSuperset(SchemaNode other)
    {
        if (other is not EntityNode en)
        {
            return false;
        }

        if (!EnumeratedValuesNodeData.IsSuperset(en.EnumeratedValuesNodeData))
            return false;

        if (!EntityAdditionalItems.AdditionalItems.IsSuperset(
                en.EntityAdditionalItems.AdditionalItems
            ))
            return false;

        var r = EntityPropertiesData.IsSuperset(
            en.EntityPropertiesData,
            EntityAdditionalItems.AdditionalItems
        );

        return r;
    }

    /// <inheritdoc />
    public override Maybe<TypeReference> ToTypeReference() => new TypeReference.Entity(this);

    /// <summary>
    /// Gets the type reference of a particular property
    /// </summary>
    public Maybe<TypeReference> GetPropertyTypeReference(EntityPropertyKey epk)
    {
        var (key, remainder) = epk.Split();

        var childNode = EntityPropertiesData.Nodes.TryGetValue(key, out var child)
            ? child.Node
            : EntityAdditionalItems.AdditionalItems;

        if (remainder.HasValue)
        {
            return childNode switch
            {
                EntityNode en => en.GetPropertyTypeReference(remainder.Value),
                TrueNode      => TypeReference.Dynamic.Instance,
                _             => Maybe<TypeReference>.None
            };
        }
        else
        {
            return childNode.ToTypeReference();
        }
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        if (value is not Entity nestedEntity)
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should be Entity", propertyName);

        var errors = new List<IErrorBuilder>();

        var remainingRequiredProperties =
            EntityPropertiesData.Nodes.Where(x => x.Value.Required)
                .Select(x => x.Key)
                .ToHashSet();

        var newEntity = nestedEntity;
        var changed   = false;

        bool allowExtra;

        if (transformSettings.RemoveExtra.HasValue)
            allowExtra = transformSettings.RemoveExtra.GetValueOrThrow();
        else
        {
            allowExtra = EntityAdditionalItems.AdditionalItems
                .TryTransform(propertyName, value, transformSettings)
                .IsSuccess;
        }

        foreach (var entityProperty in nestedEntity)
        {
            if (entityProperty.Value.IsEmpty())
            {
                changed   = true;
                newEntity = newEntity.RemoveProperty(entityProperty.Name);
                continue;
            }

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
                        node.Order
                    );

                    changed = true;
                }
                else if (entityProperty.Order != node.Order)
                {
                    newEntity = newEntity.WithProperty(
                        entityProperty.Name,
                        entityProperty.Value,
                        node.Order
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
            return Result.Failure<Maybe<ISCLObject>, IErrorBuilder>(
                ErrorBuilderList.Combine(errors)
            );

        if (changed)
            return Maybe<ISCLObject>.From(new Entity(newEntity));

        return Maybe<ISCLObject>.None;
    }
}
