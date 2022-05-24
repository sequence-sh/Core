namespace Reductech.Sequence.Core.Entities.Schema;

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
    public override bool IsSuperset(SchemaNode other)
    {
        if (other is NumberNode numberNode
         && EnumeratedValuesNodeData.IsSuperset(other.EnumeratedValuesNodeData)
         && Restrictions.IsSuperset(numberNode.Restrictions)
           )
            return true;

        if (other is IntegerNode integerNode
         && EnumeratedValuesNodeData.IsSuperset(other.EnumeratedValuesNodeData)
         && Restrictions.IsSuperset(integerNode.Restrictions))
            return true;

        return false;
    }

    /// <inheritdoc />
    protected override bool CanCombineWith(SchemaNode other)
    {
        return other is NumberNode or IntegerNode;
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

            return Maybe<ISCLObject>.None; //Integer is also number
        }
        else if (value is SCLDouble evDouble)

        {
            var restrictionResult = Restrictions.Test(evDouble.Value, propertyName);

            if (restrictionResult.IsFailure)
                return restrictionResult.ConvertFailure<Maybe<ISCLObject>>();

            return Maybe<ISCLObject>.None;
        }

        var v = value.Serialize(SerializeOptions.Primitive);

        if (!double.TryParse(v, out var d))
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Number", propertyName);

        var restrictionResult2 = Restrictions.Test(d, propertyName);

        if (restrictionResult2.IsFailure)
            return restrictionResult2.ConvertFailure<Maybe<ISCLObject>>();

        return Maybe<ISCLObject>.From(new SCLDouble(d));
    }
}
