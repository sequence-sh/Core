using System.Text.RegularExpressions;

namespace Reductech.Sequence.Core.Entities.Schema;

/// <summary>
/// Data for a node
/// </summary>
public interface INodeData
{
    /// <summary>
    /// Set the schema builder with this node data
    /// </summary>
    /// <param name="builder"></param>
    void SetBuilder(JsonSchemaBuilder builder);
}

/// <summary>
/// Contains additional data about a node
/// </summary>
public abstract record NodeData<T> : INodeData where T : NodeData<T>
{
    /// <summary>
    /// Try to combine with another NodeData of the same type
    /// </summary>
    [Pure]
    public abstract T Combine(T other);

    /// <summary>
    /// Set a JsonSchemaBuilder with this data
    /// </summary>
    public abstract void SetBuilder(JsonSchemaBuilder builder);
}

/// <summary>
/// Restrictions on a String Node
/// </summary>
public record StringRestrictions
    (uint? MinLength, uint? MaxLength, Regex? PatternRegex) : NodeData<StringRestrictions>
{
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
    public Result<Unit, IErrorBuilder> Test(string value, string propertyName)
    {
        if (MinLength.HasValue && value.Length < MinLength)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should have length >= {MinLength}",
                propertyName
            );

        if (MaxLength.HasValue && value.Length > MaxLength)
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should have length <= {MaxLength}",
                propertyName
            );

        if (PatternRegex is not null && !PatternRegex.IsMatch(value))
        {
            return ErrorCode.SchemaViolation.ToErrorBuilder(
                $"Should match Regex: {PatternRegex}",
                propertyName
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
