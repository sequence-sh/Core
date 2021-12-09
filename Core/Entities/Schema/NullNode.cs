namespace Reductech.EDR.Core.Entities.Schema;

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
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        if (entityValue is EntityValue.Null)
            return Maybe<EntityValue>.None;

        var nullWords = transformSettings.NullFormatter.GetFormats(propertyName);

        var v = entityValue.GetPrimitiveString();

        if (nullWords.Contains(v))
            return Maybe<EntityValue>.From(EntityValue.Null.Instance);

        return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Null", propertyName);
    }
}
