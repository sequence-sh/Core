namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// A schema node with one additional item of data
/// </summary>
public abstract record SchemaNode<TData1, TData2>(
    EnumeratedValuesNodeData EnumeratedValuesNodeData,
    TData1 Data1,
    TData2 Data2) : SchemaNode(EnumeratedValuesNodeData)
    where TData1 : NodeData<TData1>
    where TData2 : NodeData<TData2>
{
    /// <inheritdoc />
    public override IEnumerable<INodeData> NodeData
    {
        get
        {
            yield return EnumeratedValuesNodeData;
            yield return Data1;
            yield return Data2;
        }
    }

    /// <inheritdoc />
    public override SchemaNode Combine(SchemaNode other)
    {
        if (other.IsSuperset(this))
            return other;

        if (IsSuperset(other))
            return this;

        if (other is SchemaNode<TData1, TData2> otherNode)
        {
            if (CanCombineWith(otherNode))
            {
                var evndCombined =
                    EnumeratedValuesNodeData.Combine(otherNode.EnumeratedValuesNodeData);

                var data1Combined = Data1.Combine(otherNode.Data1);
                var data2Combined = Data2.Combine(otherNode.Data2);

                return this with
                {
                    EnumeratedValuesNodeData = evndCombined,
                    Data1 = data1Combined,
                    Data2 = data2Combined
                };
            }

            if (otherNode.CanCombineWith(this))
            {
                var evndCombined =
                    otherNode.EnumeratedValuesNodeData.Combine(EnumeratedValuesNodeData);

                var data1Combined = otherNode.Data1.Combine(Data1);
                var data2Combined = otherNode.Data2.Combine(Data2);

                return otherNode with
                {
                    EnumeratedValuesNodeData = evndCombined,
                    Data1 = data1Combined,
                    Data2 = data2Combined
                };
            }
        }

        return TrueNode.Instance;
    }
}

/// <summary>
/// A schema node with one additional item of data
/// </summary>
public abstract record SchemaNode<TData1>(
    EnumeratedValuesNodeData EnumeratedValuesNodeData,
    TData1 Data1) : SchemaNode(EnumeratedValuesNodeData) where TData1 : NodeData<TData1>
{
    /// <inheritdoc />
    public override IEnumerable<INodeData> NodeData
    {
        get
        {
            yield return EnumeratedValuesNodeData;
            yield return Data1;
        }
    }

    /// <inheritdoc />
    public override SchemaNode Combine(SchemaNode other)
    {
        if (other.IsSuperset(this))
            return other;

        if (IsSuperset(other))
            return this;

        if (other is SchemaNode<TData1> otherNode)
        {
            if (this.CanCombineWith(otherNode))
            {
                var evndCombined =
                    EnumeratedValuesNodeData.Combine(otherNode.EnumeratedValuesNodeData);

                var dataCombined = Data1.Combine(otherNode.Data1);

                return this with { EnumeratedValuesNodeData = evndCombined, Data1 = dataCombined };
            }
            else if (otherNode.CanCombineWith(this))
            {
                var evndCombined =
                    otherNode.EnumeratedValuesNodeData.Combine(EnumeratedValuesNodeData);

                var dataCombined = otherNode.Data1.Combine(Data1);

                return otherNode with
                {
                    EnumeratedValuesNodeData = evndCombined, Data1 = dataCombined
                };
            }
        }

        return TrueNode.Instance;
    }
}

/// <summary>
/// A Node in a schema
/// </summary>
public abstract record SchemaNode(EnumeratedValuesNodeData EnumeratedValuesNodeData)
{
    /// <summary>
    /// Convert this SchemaNode to a type reference. Returns None if this is a False Node
    /// </summary>
    public abstract Maybe<TypeReference> ToTypeReference();

    /// <summary>
    /// Create a schema node from a Json Schema
    /// </summary>
    [Pure]
    public static SchemaNode Create(JsonSchema schema)
    {
        if (schema.Keywords is null)
            return TrueNode.Instance;

        var type = schema.Keywords.OfType<TypeKeyword>().FirstOrDefault()
                ?? new TypeKeyword(SchemaValueType.Object);

        EnumeratedValuesNodeData enumeratedValuesNodeData;

        var constantValue = schema.Keywords.OfType<ConstKeyword>()
            .Select(x => x.Value.ToJsonDocument()?.RootElement)
            .FirstOrDefault();

        if (constantValue is not null)
            enumeratedValuesNodeData =
                new EnumeratedValuesNodeData(new[] { Entity.Create(constantValue.Value), });
        else
        {
            var enumValues = schema.Keywords.OfType<EnumKeyword>()
                .Select(x => x.Values)
                .FirstOrDefault();

            if (enumValues is not null)
                enumeratedValuesNodeData =
                    new EnumeratedValuesNodeData(
                        enumValues.WhereNotNull()
                            .Select(Entity.Create)
                            .ToList()
                    );

            else
                enumeratedValuesNodeData = EnumeratedValuesNodeData.Empty;
        }

        switch (type.Type)
        {
            case SchemaValueType.Object:
            {
                var allowExtra = schema.Keywords?.OfType<AdditionalPropertiesKeyword>()
                    .Select(x => Create(x.Schema))
                    .FirstOrDefault() ?? TrueNode.Instance;

                var requiredProperties =
                    schema.Keywords?.OfType<RequiredKeyword>()
                        .SelectMany(x => x.Properties)
                        .ToHashSet() ?? new HashSet<string>();

                var order = 0;

                var nodes = schema.Keywords!.OfType<PropertiesKeyword>()
                    .SelectMany(x => x.Properties)
                    .ToDictionary(
                        x => x.Key,
                        x => (
                            Create(x.Value),
                            requiredProperties.Contains(x.Key), order++),
                        StringComparer.OrdinalIgnoreCase
                    );

                return new EntityNode(
                    enumeratedValuesNodeData,
                    new EntityAdditionalItems(allowExtra),
                    new EntityPropertiesData(nodes)
                );
            }
            case SchemaValueType.Array:
            {
                var prefixItems = schema.Keywords!.OfType<PrefixItemsKeyword>()
                    .SelectMany(x => x.ArraySchemas)
                    .Select(Create)
                    .ToImmutableList();

                var additionalItems = schema.Keywords!.OfType<ItemsKeyword>()
                    .Select(x => x.SingleSchema)
                    .Where(x => x != null)
                    .Select(Create!)
                    .FirstOrDefault() ?? TrueNode.Instance;

                return new ArrayNode(
                    enumeratedValuesNodeData,
                    new ItemsData(prefixItems, additionalItems)
                );
            }
            case SchemaValueType.Boolean: return BooleanNode.Default;
            case SchemaValueType.String:
            {
                var format = StringFormat.Create(
                    schema.Keywords!.OfType<FormatKeyword>()
                        .Select(x => x.Value.Key)
                        .FirstOrDefault()
                );

                var restrictions = StringRestrictions.Create(schema);

                return new StringNode(enumeratedValuesNodeData, format, restrictions);
            }
            case SchemaValueType.Number:
                return new IntegerNode(enumeratedValuesNodeData, NumberRestrictions.Create(schema));
            case SchemaValueType.Integer:
                return new IntegerNode(enumeratedValuesNodeData, NumberRestrictions.Create(schema));
            case SchemaValueType.Null: return NullNode.Instance;
            default:                   throw new ArgumentOutOfRangeException(type.Type.ToString());
        }
    }

    /// <summary>
    /// Convert this to a Json Schema
    /// </summary>
    /// <returns></returns>
    [Pure]
    public JsonSchema ToJsonSchema()
    {
        var builder = new JsonSchemaBuilder().Type(SchemaValueType);

        foreach (var nodeData in NodeData)
        {
            nodeData.SetBuilder(builder);
        }

        return builder.Build();
    }

    /// <summary>
    /// Gets all node data for this Schema
    /// </summary>
    [Pure]
    public abstract IEnumerable<INodeData> NodeData { get; }

    /// <summary>
    /// The Schema Value type
    /// </summary>
    [Pure]
    public abstract SchemaValueType SchemaValueType { get; }

    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    [Pure]
    public abstract bool IsSuperset(SchemaNode other);

    /// <summary>
    /// Can this combine with the other node
    /// </summary>
    [Pure]
    protected virtual bool CanCombineWith(SchemaNode other)
    {
        return GetType() == other.GetType();
    }

    /// <summary>
    /// Try to combine this node with another schema node
    /// </summary>
    [Pure]
    public virtual SchemaNode Combine(SchemaNode otherNode)
    {
        if (otherNode.IsSuperset(this))
            return otherNode;

        if (IsSuperset(otherNode))
            return this;

        if (CanCombineWith(otherNode))
        {
            var combineResult = EnumeratedValuesNodeData
                .Combine(otherNode.EnumeratedValuesNodeData);

            return this with { EnumeratedValuesNodeData = combineResult };
        }
        else if (otherNode.CanCombineWith(this))
        {
            var combineResult = otherNode.EnumeratedValuesNodeData
                .Combine(EnumeratedValuesNodeData);

            return otherNode with { EnumeratedValuesNodeData = combineResult };
        }

        return TrueNode.Instance;
    }

    /// <summary>
    /// Try to transform this entity to match this schema
    /// </summary>
    [Pure]
    public Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform(
        string propertyName,
        ISCLObject entityValue,
        TransformSettings transformSettings)
    {
        if (!EnumeratedValuesNodeData.Allow(entityValue, transformSettings))
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Value not allowed: {entityValue}",
                propertyName
            );

        var r = TryTransform1(propertyName, entityValue, transformSettings);

        return r;
    }

    /// <summary>
    /// Try to transform the entity value
    /// </summary>
    protected abstract Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings);
}
