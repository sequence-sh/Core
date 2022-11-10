namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Restricts number values
/// </summary>
public record NumberRestrictions(
    double? Min = null,
    double? Max = null,
    double? ExclusiveMin = null,
    double? ExclusiveMax = null,
    double? MultipleOf = null) : NodeData<NumberRestrictions>
{
    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public bool IsSuperset(NumberRestrictions other)
    {
        if (this == NoRestrictions)
            return true;

        if (other == NoRestrictions)
            return false;

        if (Min.HasValue && (other.Min is null || other.Min < Min))
            return false;

        if (Max.HasValue && (other.Max is null || other.Max > Max))
            return false;

        if (ExclusiveMin.HasValue
         && (other.ExclusiveMin is null || other.ExclusiveMin < ExclusiveMin))
            return false;

        if (ExclusiveMax.HasValue
         && (other.ExclusiveMax is null || other.ExclusiveMax > ExclusiveMax))
            return false;

        if (MultipleOf.HasValue && (other.MultipleOf is null || other.MultipleOf % MultipleOf != 0))
            return false;

        return true;
    }

    /// <summary>
    /// Create a new NumberRestrictions
    /// </summary>
    public static NumberRestrictions Create(JsonSchema schema)
    {
        if (schema.Keywords is null)
            return NoRestrictions;

        var min = schema.Keywords.OfType<MinimumKeyword>()
            .Select(x => Convert.ToDouble(x.Value) as double?)
            .FirstOrDefault();

        var max = schema.Keywords.OfType<MaximumKeyword>()
            .Select(x => Convert.ToDouble(x.Value) as double?)
            .FirstOrDefault();

        var exclusiveMin = schema.Keywords.OfType<ExclusiveMinimumKeyword>()
            .Select(x => Convert.ToDouble(x.Value) as double?)
            .FirstOrDefault();

        var exclusiveMax = schema.Keywords.OfType<ExclusiveMaximumKeyword>()
            .Select(x => Convert.ToDouble(x.Value) as double?)
            .FirstOrDefault();

        var multipleOf = schema.Keywords.OfType<MultipleOfKeyword>()
            .Select(x => Convert.ToDouble(x.Value) as double?)
            .FirstOrDefault();

        var any = min.HasValue || max.HasValue || exclusiveMin.HasValue || exclusiveMax.HasValue
               || multipleOf.HasValue;

        if (any)
            return new(min, max, exclusiveMin, exclusiveMax, multipleOf);

        return NoRestrictions;
    }

    /// <summary>
    /// No Number Restrictions
    /// </summary>
    public static readonly NumberRestrictions NoRestrictions = new();

    /// <summary>
    /// Test if these restrictions are met
    /// </summary>
    public Result<Unit, IErrorBuilder> Test(int i, string propertyName, TransformRoot transformRoot)
    {
        if (i < Min)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be >= {Min}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (i > Max)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be <= {Max}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (i <= ExclusiveMin)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be > {ExclusiveMin}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (i <= ExclusiveMax)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be < {ExclusiveMax}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (MultipleOf.HasValue)
        {
            if (MultipleOf.Value == 0)
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    $"Cannot be a multiple of 0",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );

            var rem = i % MultipleOf.Value;

            if (rem != 0)
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    $"Should be a multiple of {MultipleOf}",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );
        }

        return Unit.Default;
    }

    /// <summary>
    /// Test if these restrictions are met
    /// </summary>
    public Result<Unit, IErrorBuilder> Test(
        double d,
        string propertyName,
        TransformRoot transformRoot)
    {
        if (d < Min)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be >= {Min}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (d > Max)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be <= {Max}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (d <= ExclusiveMin)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be > {ExclusiveMin}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (d <= ExclusiveMax)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should be < {ExclusiveMax}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (MultipleOf.HasValue)
        {
            if (MultipleOf.Value == 0)
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    $"Cannot be a multiple of 0",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );

            var rem = d % MultipleOf.Value;

            if (rem != 0)
                return ErrorCode.SchemaViolated.ToErrorBuilder(
                    $"Should be a multiple of {MultipleOf}",
                    propertyName,
                    transformRoot.RowNumber,
                    transformRoot.Entity
                );
        }

        return Unit.Default;
    }

    /// <inheritdoc />
    public override NumberRestrictions Combine(NumberRestrictions other)
    {
        return new NumberRestrictions(
            CombineHelpers.Combine(Min,          other.Min,          (d, d1) => Math.Min(d, d1)),
            CombineHelpers.Combine(Max,          other.Max,          (d, d1) => Math.Max(d, d1)),
            CombineHelpers.Combine(ExclusiveMin, other.ExclusiveMin, (d, d1) => Math.Min(d, d1)),
            CombineHelpers.Combine(ExclusiveMax, other.ExclusiveMax, (d, d1) => Math.Max(d, d1)),
            CombineHelpers.Combine(
                MultipleOf,
                other.MultipleOf,
                (d, d1) => LowestCommonMultiple(d, d1)
            )
        );

        static double LowestCommonMultiple(double a, double b)
        {
            return (a / GreatestCommonFactor(a, b)) * b;

            static double GreatestCommonFactor(double a, double b)
            {
                while (b != 0)
                {
                    var temp = b;
                    b = a % b;
                    a = temp;
                }

                return a;
            }
        }
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        if (Min.HasValue)
            builder.Minimum(Convert.ToDecimal(Min.Value));

        if (Max.HasValue)
            builder.Maximum(Convert.ToDecimal(Max.Value));

        if (ExclusiveMin.HasValue)
            builder.ExclusiveMinimum(Convert.ToDecimal(ExclusiveMin.Value));

        if (ExclusiveMax.HasValue)
            builder.ExclusiveMaximum(Convert.ToDecimal(ExclusiveMax.Value));

        if (MultipleOf.HasValue)
            builder.MultipleOf(Convert.ToDecimal(MultipleOf.Value));
    }
}
