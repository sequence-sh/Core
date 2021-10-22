using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema
{

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
    public override bool IsMorePermissive(SchemaNode other)
    {
        return true;
    }

    /// <inheritdoc />
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        return Maybe<EntityValue>.None;
    }
}

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
        return ErrorCode.SchemaViolation.ToErrorBuilder("Always False", propertyName);
    }
}

}
