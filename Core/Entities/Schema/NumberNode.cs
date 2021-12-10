namespace Reductech.EDR.Core.Entities.Schema;

/// <summary>
/// Matches numbers
/// </summary>
public record NumberNode(
        EnumeratedValuesNodeData EnumeratedValuesNodeData,
        NumberRestrictions Restrictions)
    : SchemaNode<NumberRestrictions>(EnumeratedValuesNodeData, Restrictions)
{
    /// <summary>
    /// The default NumberNode
    /// </summary>
    public static NumberNode Default { get; } = new(
        EnumeratedValuesNodeData.Empty,
        NumberRestrictions.NoRestrictions
    );

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Number;

    /// <inheritdoc />
    public override bool IsMorePermissive(SchemaNode other)
    {
        return false;
    }

    /// <inheritdoc />
    protected override bool CanCombineWith(SchemaNode other)
    {
        return other is NumberNode or IntegerNode;
    }

    /// <inheritdoc />
    protected override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform1(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        if (entityValue is EntityValue.Integer evInteger)
        {
            var restrictionResult = Restrictions.Test(evInteger.Value, propertyName);

            if (restrictionResult.IsFailure)
                return restrictionResult.ConvertFailure<Maybe<EntityValue>>();

            return Maybe<EntityValue>.None; //Integer is also number
        }
        else if (entityValue is EntityValue.Double evDouble)

        {
            var restrictionResult = Restrictions.Test(evDouble.Value, propertyName);

            if (restrictionResult.IsFailure)
                return restrictionResult.ConvertFailure<Maybe<EntityValue>>();

            return Maybe<EntityValue>.None;
        }

        var v = entityValue.GetPrimitiveString();

        if (!double.TryParse(v, out var d))
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Number", propertyName);

        var restrictionResult2 = Restrictions.Test(d, propertyName);

        if (restrictionResult2.IsFailure)
            return restrictionResult2.ConvertFailure<Maybe<EntityValue>>();

        return Maybe<EntityValue>.From(new EntityValue.Double(d));
    }
}
