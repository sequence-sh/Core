namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// Freezes into a create entity step
/// </summary>
public sealed record CreateEntityFreezableStep
    (FreezableEntityData FreezableEntityData) : IFreezableStep
{
    /// <inheritdoc />
    public string StepName => "Create Entity";

    /// <inheritdoc />
    public TextLocation TextLocation => FreezableEntityData.Location;

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var checkResult = callerMetadata.CheckAllows(
                TypeReference.Entity.NoSchema,
                typeResolver
            )
            .MapError(x => x.WithLocation(this));

        if (checkResult.IsFailure)
            return checkResult.ConvertFailure<IStep>();

        var results = new List<Result<(EntityPropertyKey name, IStep value), IError>>();

        foreach (var (propertyName, stepMember) in FreezableEntityData.EntityProperties)
        {
            var cm = new CallerMetadata(
                StepName,
                propertyName.AsString,
                TypeReference.Any.Instance
            );

            var frozen = stepMember.ConvertToStep()
                .TryFreeze(cm, typeResolver)
                .Map(s => (propertyName, s));

            results.Add(frozen);
        }

        var r =
            results.Combine(ErrorList.Combine)
                .Map(
                    v =>
                        v.ToDictionary(x => x.name, x => x.value)
                );

        if (r.IsFailure)
            return r.ConvertFailure<IStep>();

        return new CreateEntityStep(r.Value) { TextLocation = TextLocation };
    }

    /// <inheritdoc />
    public UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        var checkResult = callerMetadata.CheckAllows(
                TypeReference.Entity.NoSchema,
                typeResolver
            )
            .MapError(x => x.WithLocation(this));

        if (checkResult.IsFailure)
            return checkResult.ConvertFailure<Unit>();

        var results = new List<UnitResult<IError>>();

        foreach (var (propertyName, stepMember) in FreezableEntityData.EntityProperties)
        {
            var cm = new CallerMetadata(
                StepName,
                propertyName.AsString,
                TypeReference.Any.Instance
            );

            var result = stepMember.ConvertToStep()
                .CheckFreezePossible(cm, typeResolver);

            results.Add(result);
        }

        if (results.All(x => x.IsSuccess))
            return UnitResult.Success<IError>();

        return UnitResult.Failure(
            ErrorList.Combine(results.Where(x => x.IsFailure).Select(x => x.Error))
        );
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return FreezableEntityData.GetVariablesUsed(callerMetadata, typeResolver);
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        var entityNode = CreateEntityNode(typeResolver);

        return new TypeReference.Entity(entityNode);
    }

    private EntityNode CreateEntityNode(TypeResolver typeResolver)
    {
        var dict =
            new Dictionary<string, (SchemaNode Node, bool Required, int Order)>(
                StringComparer.OrdinalIgnoreCase
            );

        var order = 0;

        foreach (var kvp in FreezableEntityData.EntityProperties)
        {
            var (key, remainder) = kvp.Key.Split();

            var type =
                kvp.Value.ConvertToStep()
                    .TryGetOutputTypeReference(
                        new CallerMetadata("Entity", kvp.Key.AsString, TypeReference.Any.Instance),
                        typeResolver
                    )
                    .ToMaybe()
                    .GetValueOrDefault(TypeReference.Any.Instance);

            var newNode = type.ToSchemaNode(typeResolver);

            if (remainder.HasNoValue)
            {
                dict[key] = (newNode, true, order);
                order++;
            }
            else if (dict.TryGetValue(key, out var existing))
            {
                EntityNode entityNode;

                if (existing.Node is EntityNode en)
                {
                    entityNode = en;
                }
                else
                {
                    entityNode = new EntityNode(
                        EnumeratedValuesNodeData.Empty,
                        new EntityAdditionalItems(FalseNode.Instance),
                        EntityPropertiesData.Empty
                    );

                    entityNode = UpdateEntityNode(
                        entityNode,
                        new EntityPropertyKey(Entity.PrimitiveKey),
                        existing.Node
                    );
                }

                var updated = UpdateEntityNode(entityNode, remainder.Value, newNode);

                dict[key] = (updated, existing.Required, existing.Order);
            }
            else
            {
                var entityNode = new EntityNode(
                    EnumeratedValuesNodeData.Empty,
                    new EntityAdditionalItems(FalseNode.Instance),
                    EntityPropertiesData.Empty
                );

                entityNode = UpdateEntityNode(entityNode, remainder.Value, newNode);
                dict[key]  = (entityNode, true, order);
                order++;
            }

            static EntityNode UpdateEntityNode(
                EntityNode node,
                EntityPropertyKey key,
                SchemaNode schemaNode)
            {
                var (start, remaining) = key.Split();

                if (remaining.HasNoValue)
                {
                    var dict =
                        node.EntityPropertiesData.Nodes.ToDictionary(x => x.Key, x => x.Value);

                    dict[start] = (schemaNode, true, dict.Count);

                    node = new EntityNode(
                        node.EnumeratedValuesNodeData,
                        node.EntityAdditionalItems,
                        new EntityPropertiesData(dict)
                    );

                    return node;
                }

                return UpdateEntityNode(node, remaining.Value, schemaNode);
            }
        }

        return new EntityNode(
            EnumeratedValuesNodeData.Empty,
            EntityAdditionalItems: new EntityAdditionalItems(FalseNode.Instance),
            new EntityPropertiesData(dict)
        );
    }

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore)
    {
        var dict = new Dictionary<EntityPropertyKey, FreezableStepProperty>();

        foreach (var (key, value) in FreezableEntityData.EntityProperties)
        {
            var r = value.ReorganizeNamedArguments(stepFactoryStore);
            dict.Add(key, r);
        }

        return new CreateEntityFreezableStep(FreezableEntityData with { EntityProperties = dict });
    }

    /// <inheritdoc />
    bool IEquatable<IFreezableStep>.Equals(IFreezableStep? other) =>
        other is CreateEntityFreezableStep oStep
     && FreezableEntityData.Equals(oStep.FreezableEntityData);
}
