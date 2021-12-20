namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Matches Integers
/// </summary>
public record IntegerNode(
    EnumeratedValuesNodeData EnumeratedValuesNodeData,
    NumberRestrictions Restrictions) : SchemaNode<NumberRestrictions>(
    EnumeratedValuesNodeData,
    Restrictions
)
{
    /// <summary>
    /// The Default integer node
    /// </summary>
    public static IntegerNode Default { get; } = new(
        EnumeratedValuesNodeData.Empty,
        NumberRestrictions.NoRestrictions
    );

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Integer;

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
        if (value is SCLInt evInteger)
        {
            var restrictionResult = Restrictions.Test(evInteger.Value, propertyName);

            if (restrictionResult.IsFailure)
                return restrictionResult.ConvertFailure<Maybe<ISCLObject>>();

            return Maybe<ISCLObject>.None; // No change
        }

        var v = value.Serialize(SerializeOptions.Primitive);

        if (!int.TryParse(v, out var i))
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Integer", propertyName);

        var restrictionResult2 = Restrictions.Test(i, propertyName);

        if (restrictionResult2.IsFailure)
            return restrictionResult2.ConvertFailure<Maybe<ISCLObject>>();

        return Maybe<ISCLObject>.From(new SCLInt(i));
    }
}
