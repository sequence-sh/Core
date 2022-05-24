namespace Reductech.Sequence.Core.Entities.Schema;

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
    public override bool IsSuperset(SchemaNode other)
    {
        if (!EnumeratedValuesNodeData.IsSuperset(other.EnumeratedValuesNodeData))
            return false;

        if (other is StringNode otherStringNode)
        {
            var r = Format.IsSuperset(otherStringNode.Format)
                 && StringRestrictions.IsSuperset(otherStringNode.StringRestrictions);

            return r;
        }

        if (Format is AnyStringFormat && StringRestrictions == StringRestrictions.NoRestrictions)
        {
            if (other is NumberNode or IntegerNode or NullNode)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings)
    {
        var r1 = Format.TryTransform(propertyName, value, transformSettings);

        if (r1.IsFailure)
            return r1;

        if (StringRestrictions == StringRestrictions.NoRestrictions)
            return r1;

        var s = r1.Value.GetValueOrDefault(value).Serialize(SerializeOptions.Primitive);

        var testResult = StringRestrictions.Test(s, propertyName);

        if (testResult.IsFailure)
            return testResult.ConvertFailure<Maybe<ISCLObject>>();

        return r1;
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.String;
}
