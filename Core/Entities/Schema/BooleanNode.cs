namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Schema is matched by a boolean value
/// </summary>
public record BooleanNode(EnumeratedValuesNodeData EnumeratedValuesNodeData) : SchemaNode(
    EnumeratedValuesNodeData
)
{
    /// <summary>
    /// The default NumberNode
    /// </summary>
    public static BooleanNode Default { get; } = new(EnumeratedValuesNodeData.Empty);

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
        if (entityValue is EntityValue.Boolean)
            return Maybe<EntityValue>.None;

        var trueWords  = transformSettings.TruthFormatter.GetFormats(propertyName);
        var falseWords = transformSettings.FalseFormatter.GetFormats(propertyName);

        var v = entityValue.GetPrimitiveString();

        if (trueWords.Contains(v))
            return Maybe<EntityValue>.From(new EntityValue.Boolean(true));

        if (falseWords.Contains(v))
            return Maybe<EntityValue>.From(new EntityValue.Boolean(false));

        return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Boolean", propertyName);
    }
}
