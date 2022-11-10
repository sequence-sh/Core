using System.Text.RegularExpressions;

namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Restrictions on a String Node
/// </summary>
public record StringRestrictions
    (uint? MinLength, uint? MaxLength, Regex? PatternRegex) : NodeData<StringRestrictions>
{
    /// <summary>
    /// Are the allowed values a superset (not strict) of the allowed values of the other node.
    /// </summary>
    public bool IsSuperset(StringRestrictions other)
    {
        if (this == NoRestrictions)
            return true;

        if (other == NoRestrictions)
            return false;

        if (MinLength.HasValue && (!other.MinLength.HasValue || other.MinLength > MinLength))
            return false;

        if (MaxLength.HasValue && (!other.MaxLength.HasValue || other.MaxLength < MaxLength))
            return false;

        if (PatternRegex is not null
         && (other.PatternRegex is null || other.PatternRegex != PatternRegex))
        {
            //In principle this could be improved but I'm not going to
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override StringRestrictions Combine(StringRestrictions other)
    {
        if (other == NoRestrictions)
            return this;

        if (this == NoRestrictions)
            return other;

        if (this == other)
            return this;

        var restrictions = new StringRestrictions(
            CombineHelpers.Combine(
                MinLength,
                other.MinLength,
                (a, b) => Math.Max(a, b)
            ), //We need the highest min length
            CombineHelpers.Combine(
                MaxLength,
                other.MaxLength,
                (a, b) => Math.Min(a, b)
            ), // We need the lowest max length
            CombineHelpers.Combine(
                PatternRegex,
                other.PatternRegex,
                (a, _) => a
            ) //TODO improve this
        );    //this is wrong

        return restrictions;
    }

    /// <summary>
    /// Create a new StringRestrictions
    /// </summary>
    [Pure]
    public static StringRestrictions Create(JsonSchema schema)
    {
        if (schema.Keywords is null)
            return NoRestrictions;

        var minLength = schema.Keywords.OfType<MinLengthKeyword>()
            .Select(x => x.Value as uint?)
            .FirstOrDefault();

        var maxLength = schema.Keywords.OfType<MaxLengthKeyword>()
            .Select(x => x.Value as uint?)
            .FirstOrDefault();

        var pattern = schema.Keywords.OfType<PatternKeyword>()
            .Select(x => x.Value)
            .FirstOrDefault();

        var any = minLength.HasValue || maxLength.HasValue || pattern is not null;

        if (any)
            return new(minLength, maxLength, pattern);

        return NoRestrictions;
    }

    /// <summary>
    /// Test if these restrictions are met
    /// </summary>
    public Result<Unit, IErrorBuilder> Test(
        string value,
        string propertyName,
        TransformRoot transformRoot)
    {
        if (MinLength.HasValue && value.Length < MinLength)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should have length >= {MinLength}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (MaxLength.HasValue && value.Length > MaxLength)
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should have length <= {MaxLength}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );

        if (PatternRegex is not null && !PatternRegex.IsMatch(value))
        {
            return ErrorCode.SchemaViolated.ToErrorBuilder(
                $"Should match Regex: {PatternRegex}",
                propertyName,
                transformRoot.RowNumber,
                transformRoot.Entity
            );
        }

        return Unit.Default;
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        if (MinLength.HasValue)
            builder.MinLength(MinLength.Value);

        if (MaxLength.HasValue)
            builder.MaxLength(MaxLength.Value);

        if (PatternRegex is not null)
            builder.Pattern(PatternRegex);
    }

    /// <summary>
    /// No String Restrictions
    /// </summary>
    public static readonly StringRestrictions NoRestrictions = new(null, null, null);
}
