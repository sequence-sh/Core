namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Schema is matched by a boolean value
/// </summary>
public record BooleanNode(EnumeratedValuesNodeData EnumeratedValuesNodeData) : SchemaNode(
    EnumeratedValuesNodeData
)
{
    /// <summary>
    /// The default BooleanNode
    /// </summary>
    public static BooleanNode Default { get; } = new(EnumeratedValuesNodeData.Empty);

    /// <inheritdoc />
    public override Maybe<TypeReference> ToTypeReference()
    {
        return TypeReference.Actual.Bool;
    }

    /// <inheritdoc />
    public override IEnumerable<INodeData> NodeData
    {
        get
        {
            yield return EnumeratedValuesNodeData;
        }
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Boolean;

    /// <inheritdoc />
    public override bool IsSuperset(SchemaNode other)
    {
        return other is BooleanNode
            && EnumeratedValuesNodeData.IsSuperset(EnumeratedValuesNodeData);
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        if (value is SCLBool)
            return Maybe<ISCLObject>.None;

        var v = value.Serialize(SerializeOptions.Primitive);

        if (transformSettings.TruthFormatter.IsMatch(
                v,
                propertyName,
                transformSettings.CaseSensitive
            ))
            return Maybe<ISCLObject>.From(SCLBool.True);

        if (transformSettings.FalseFormatter.IsMatch(
                v,
                propertyName,
                transformSettings.CaseSensitive
            ))
            return Maybe<ISCLObject>.From(SCLBool.False);

        return ErrorCode.SchemaViolated.ToErrorBuilder(
            "Should Be Boolean",
            propertyName,
            transformRoot.RowNumber,
            transformRoot.Entity
        );
    }
}
