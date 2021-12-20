namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Restricts number values
/// </summary>
public record NumberRestrictions(
    double? Min,
    double? Max,
    double? ExclusiveMin,
    double? ExclusiveMax,
    double? MultipleOf) : NodeData<NumberRestrictions>
{
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
    public static readonly NumberRestrictions NoRestrictions = new(null, null, null, null, null);

    /// <summary>
    /// Test if these restrictions are met
    /// </summary>
    public Result<Unit, IErrorBuilder> Test(int i, string propertyName)
    {
        if (i < Min)
            return ErrorCode.SchemaViolation.ToErrorBuilder($"Should be >= {Min}", propertyName);

        if (i > Max)
            return ErrorCode.SchemaViolation.ToErrorBuilder($"Should be <= {Max}", propertyName);

        if (i <= ExclusiveMin)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should be > {ExclusiveMin}",
                propertyName
            );

        if (i <= ExclusiveMax)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should be < {ExclusiveMax}",
                propertyName
            );

        if (MultipleOf.HasValue)
        {
            if (MultipleOf.Value == 0)
                return ErrorCode.SchemaViolation.ToErrorBuilder(
                    $"Cannot be a multiple of 0",
                    propertyName
                );

            var rem = i % MultipleOf.Value;

            if (rem != 0)
                return ErrorCode.SchemaViolation.ToErrorBuilder(
                    $"Should be a multiple of {MultipleOf}",
                    propertyName
                );
        }

        return Unit.Default;
    }

    /// <summary>
    /// Test if these restrictions are met
    /// </summary>
    public Result<Unit, IErrorBuilder> Test(double d, string propertyName)
    {
        if (d < Min)
            return ErrorCode.SchemaViolation.ToErrorBuilder($"Should be >= {Min}", propertyName);

        if (d > Max)
            return ErrorCode.SchemaViolation.ToErrorBuilder($"Should be <= {Max}", propertyName);

        if (d <= ExclusiveMin)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should be > {ExclusiveMin}",
                propertyName
            );

        if (d <= ExclusiveMax)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should be < {ExclusiveMax}",
                propertyName
            );

        if (MultipleOf.HasValue)
        {
            if (MultipleOf.Value == 0)
                return ErrorCode.SchemaViolation.ToErrorBuilder(
                    $"Cannot be a multiple of 0",
                    propertyName
                );

            var rem = d % MultipleOf.Value;

            if (rem != 0)
                return ErrorCode.SchemaViolation.ToErrorBuilder(
                    $"Should be a multiple of {MultipleOf}",
                    propertyName
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
