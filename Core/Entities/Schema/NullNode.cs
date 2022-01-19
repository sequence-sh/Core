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
    public override bool IsMorePermissive(SchemaNode other)
    {
        return false;
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        if (value is SCLNull)
            return Maybe<ISCLObject>.None;

        var nullWords = transformSettings.NullFormatter.GetFormats(propertyName);

        var v = value.Serialize(SerializeOptions.Primitive);

        if (nullWords.Contains(v))
            return Maybe<ISCLObject>.From(SCLNull.Instance);

        return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Null", propertyName);
    }
}
