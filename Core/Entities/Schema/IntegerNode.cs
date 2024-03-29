﻿namespace Sequence.Core.Entities.Schema;

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
    public override Maybe<TypeReference> ToTypeReference()
    {
        return TypeReference.Actual.Integer;
    }

    /// <inheritdoc />
    public override SchemaValueType SchemaValueType => SchemaValueType.Integer;

    /// <inheritdoc />
    public override bool IsSuperset(SchemaNode other)
    {
        return other is IntegerNode integerNode
            && EnumeratedValuesNodeData.IsSuperset(other.EnumeratedValuesNodeData)
            && Restrictions.IsSuperset(integerNode.Restrictions);
    }

    /// <inheritdoc />
    protected override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform1(
        string propertyName,
        ISCLObject value,
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        if (value is SCLInt evInteger)
        {
            var restrictionResult = Restrictions.Test(evInteger.Value, propertyName, transformRoot);

            if (restrictionResult.IsFailure)
                return restrictionResult.ConvertFailure<Maybe<ISCLObject>>();

            return Maybe<ISCLObject>.None; // No change
        }

        int i;

        if (value is SCLDouble evDouble)
        {
            i = (int)Math.Round(evDouble.Value);

            if (Math.Abs(evDouble.Value - i) > transformSettings.RoundingPrecision)
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    "Too far from the nearest Integer",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );
        }
        else
        {
            var v = value.Serialize(SerializeOptions.Primitive);

            if (int.TryParse(v, out i)) { }
            else if (double.TryParse(v, out var d))
            {
                i = (int)Math.Round(d);

                if (Math.Abs(d - i) > transformSettings.RoundingPrecision)
                    return ErrorCode.SchemaViolated.ToErrorBuilder(
                        "Too far from the nearest Integer",
                        propertyName,
                        transformRoot.RowNumber,
                        transformRoot.Entity
                    );
            }
            else
            {
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    "Should Be Integer",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );
            }
        }

        var restrictionResult2 = Restrictions.Test(i, propertyName, transformRoot);

        if (restrictionResult2.IsFailure)
            return restrictionResult2.ConvertFailure<Maybe<ISCLObject>>();

        return Maybe<ISCLObject>.From(new SCLInt(i));
    }
}
