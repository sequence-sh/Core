using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema;

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

            return Maybe<EntityValue>.None; // No change
        }

        var v = entityValue.GetPrimitiveString();

        if (!int.TryParse(v, out var i))
            return ErrorCode.SchemaViolation.ToErrorBuilder("Should Be Integer", propertyName);

        var restrictionResult2 = Restrictions.Test(i, propertyName);

        if (restrictionResult2.IsFailure)
            return restrictionResult2.ConvertFailure<Maybe<EntityValue>>();

        return Maybe<EntityValue>.From(new EntityValue.Integer(i));
    }
}
