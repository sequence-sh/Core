﻿namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Schema is never valid
/// </summary>
public record FalseNode() : SchemaNode(EnumeratedValuesNodeData.Empty)
{
    /// <summary>
    /// The instance
    /// </summary>
    public static FalseNode Instance { get; } = new();

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
    public override bool IsSuperset(SchemaNode other) => other is FalseNode;

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        return ErrorCode.SchemaViolation.ToErrorBuilder("Always False", propertyName);
    }
}
