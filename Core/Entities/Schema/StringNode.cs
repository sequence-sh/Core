using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Matches Strings
/// </summary>
public record StringNode(
        EnumeratedValuesNodeData EnumeratedValuesNodeData,
        StringFormat Format,
        StringRestrictions StringRestrictions)
    : SchemaNode<StringFormat, StringRestrictions>(
        EnumeratedValuesNodeData,
        Format,
        StringRestrictions
    )
{
    /// <summary>
    /// The default string node
    /// </summary>
    public static StringNode Default { get; } = new(
        EnumeratedValuesNodeData.Empty,
        AnyStringFormat.Instance,
        StringRestrictions.NoRestrictions
    );

    /// <inheritdoc />
    public override bool IsMorePermissive(SchemaNode other)
    {
        if (this != Default)
            return false;

        if (other is NumberNode or IntegerNode or StringNode or NullNode)
            return true;

        return false;
    }

    /// <inheritdoc />
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        var r1 = Format.TryTransform(propertyName, entityValue, transformSettings);

        if (r1.IsFailure)
            return r1;

        if (StringRestrictions == StringRestrictions.NoRestrictions)
            return r1;

        var s = r1.Value.GetValueOrDefault(entityValue).GetPrimitiveString();

        var testResult = StringRestrictions.Test(s, propertyName);

        if (testResult.IsFailure)
            return testResult.ConvertFailure<Maybe<EntityValue>>();

        return r1;
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.String;
}
