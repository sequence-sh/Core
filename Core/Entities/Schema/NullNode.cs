namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Matches only null
/// </summary>
public record NullNode() : SchemaNode(EnumeratedValuesNodeData.Empty)
{
    /// <summary>
    /// The instance
    /// </summary>
    public static NullNode Instance { get; } = new();

    /// <inheritdoc />
    public override IEnumerable<INodeData> NodeData
    {
        get
        {
            yield return EnumeratedValuesNodeData;
        }
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Null;

    /// <inheritdoc />
    public override bool IsSuperset(SchemaNode other)
    {
        return other is NullNode;
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        if (value is SCLNull)
            return Maybe<ISCLObject>.None;

        var v = value.Serialize(SerializeOptions.Primitive);

        if (transformSettings.NullFormatter.IsMatch(
                v,
                propertyName,
                transformSettings.CaseSensitive
            ))
            return Maybe<ISCLObject>.From(SCLNull.Instance);

        return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Null", propertyName);
    }
}
