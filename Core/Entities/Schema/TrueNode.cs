namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Schema is always valid
/// </summary>
public record TrueNode() : SchemaNode(EnumeratedValuesNodeData.Empty)
{
    /// <summary>
    /// The instance
    /// </summary>
    public static TrueNode Instance { get; } = new();

    /// <inheritdoc />
    public override Maybe<TypeReference> ToTypeReference()
    {
        return TypeReference.Any.Instance;
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
    public override SchemaValueType SchemaValueType => SchemaValueType.Object;

    /// <inheritdoc />
    public override bool IsSuperset(SchemaNode other)
    {
        return true;
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        return Maybe<ISCLObject>.None;
    }
}
