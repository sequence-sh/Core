namespace Sequence.Core.Entities.Schema;

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

    /// <summary>
    /// Gets possible keys into this entity
    /// </summary>
    public IEnumerable<(EntityNestedKey, SchemaNode)> GetKeyNodePairs()
    {
        foreach (var node in EntityPropertiesData.Nodes)
        {
            yield return (new EntityNestedKey(node.Key), node.Value.Node);

            if (node.Value.Node is EntityNode nested)
            {
                foreach (var (nestedKey, nestedNode) in nested.GetKeyNodePairs())
                {
                    yield return (
                        new EntityNestedKey(nestedKey.KeyNames.Prepend(new EntityKey(node.Key))),
                        nestedNode);
                }
            }
        }
    }

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
    public Maybe<TypeReference> GetPropertyTypeReference(EntityNestedKey epk)
    {
        var (key, remainder) = epk.Split();

        var childNode = EntityPropertiesData.Nodes.TryGetValue(key.Inner, out var child)
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
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        if (value is not Entity nestedEntity)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                "Should be Entity",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        var errors = new List<IErrorBuilder>();

        var remainingRequiredProperties =
            EntityPropertiesData.Nodes.Where(x => x.Value.Required)
                .Select(x => x.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newEntity = nestedEntity;
        var changed   = false;

        bool allowExtra;

        if (transformSettings.RemoveExtra.HasValue)
            allowExtra = transformSettings.RemoveExtra.GetValueOrThrow();
        else
        {
            allowExtra = EntityAdditionalItems.AdditionalItems
                .TryTransform(propertyName, value, transformSettings, transformRoot)
                .IsSuccess;
        }

        foreach (var (key, sclObject) in nestedEntity)
        {
            if (sclObject.IsEmpty())
            {
                changed   = true;
                newEntity = newEntity.WithPropertyRemoved(key);
                continue;
            }

            if (EntityPropertiesData.Nodes.TryGetValue(key.Inner, out var node))
            {
                var r = node.Node.TryTransform(
                    propertyName + "." + key.Inner,
                    sclObject,
                    transformSettings,
                    transformRoot
                );

                if (r.IsFailure)
                    errors.Add(r.Error);
                else
                {
                    if (r.Value.HasValue) //Change the property value{
                    {
                        newEntity = newEntity.WithPropertyAddedOrUpdated(
                            key,
                            r.Value.GetValueOrThrow()
                        );

                        changed = true;
                    }

                    var withMoved = newEntity.WithPropertyMoved(key, node.Order);

                    if (withMoved.HasValue)
                    {
                        newEntity = withMoved.Value;
                        changed   = true;
                    }
                }
            }
            else if (allowExtra)
            {
                newEntity = newEntity.WithPropertyRemoved(key);
                changed   = true;
            }

            remainingRequiredProperties.Remove(key.Inner);
        }

        foreach (var remainingRequiredProperty in remainingRequiredProperties)
        {
            errors.Add(
                ErrorCode.SchemaViolated.ToErrorBuilder(
                    $"Missing Property '{remainingRequiredProperty}'",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                )
            );
        }

        if (errors.Any())
            return Result.Failure<Maybe<ISCLObject>, IErrorBuilder>(
                ErrorBuilderList.Combine(errors)
            );

        if (changed)
            return Maybe<ISCLObject>.From(newEntity);

        return Maybe<ISCLObject>.None;
    }
}
