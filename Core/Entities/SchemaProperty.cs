using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// A single property in a a schema.
/// </summary>
[DataContract]
public sealed record SchemaProperty
{
    /// <summary>
    /// The type of the property.
    /// </summary>
    [property: DataMember]
    public SCLType Type { get; init; }

    /// <summary>
    /// If this is an enum, the name of the enum
    /// </summary>
    [property: DataMember]
    public string? EnumType { get; init; }

    /// <summary>
    /// The multiplicity of the property.
    /// </summary>
    [property: DataMember]
    public Multiplicity Multiplicity { get; init; } = Multiplicity.Any;

    /// <summary>
    /// If this is an enum, the allowed values.
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<string>? Values { get; init; }

    /// <summary>
    /// The allowed formats for the date
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<string>? DateInputFormats { get; init; }

    /// <summary>
    /// The output format for the date
    /// </summary>
    [property: DataMember]
    public string? DateOutputFormat { get; init; }

    /// <summary>
    /// A regex to validate the string form of the field value
    /// </summary>
    [property: DataMember]
    public string? Regex { get; init; }

    /// <summary>
    /// The error behavior, overriding the default value of the schema.
    /// </summary>
    [property: DataMember]
    public ErrorBehavior? ErrorBehavior { get; init; }

    /// <summary>
    /// Combines multiple schema properties
    /// </summary>
    public static Result<Maybe<SchemaProperty>, IErrorBuilder> Combine(
        string propertyName,
        IEnumerable<Maybe<SchemaProperty>> properties,
        int expectedCount)
    {
        Maybe<SchemaProperty>? current = null;
        var                    count   = 0;

        foreach (var property in properties)
        {
            if (current is null)
                current = property;
            else
            {
                var r = CombineMaybeSchemaProperties(propertyName, current.Value, property);

                if (r.IsFailure)
                    return r;

                current = r.Value;
            }

            count++;
        }

        if (current is null)
            return Maybe<SchemaProperty>.None;

        if (current.Value.HasNoValue)
            return Maybe<SchemaProperty>.None;

        if (expectedCount > count)
        {
            return current.Value.Map(AllowNone);
        }

        return current.Value;
    }

    static SchemaProperty AllowNone(SchemaProperty schemaProperty)
    {
        if (schemaProperty.Multiplicity == Multiplicity.ExactlyOne)
            return schemaProperty with { Multiplicity = Multiplicity.UpToOne };

        return schemaProperty;
    }

    private static Result<Maybe<SchemaProperty>, IErrorBuilder> CombineMaybeSchemaProperties(
        string propertyName,
        Maybe<SchemaProperty> property1,
        Maybe<SchemaProperty> property2)
    {
        if (property1.HasValue)
        {
            if (property2.HasValue)
            {
                return CombineSchemaProperties(propertyName, property1.Value, property2.Value)
                    .Map(Maybe<SchemaProperty>.From);
            }

            return property1.Map(AllowNone);
        }

        return property2.Map(AllowNone);

        static Result<SchemaProperty, IErrorBuilder> CombineSchemaProperties(
            string propertyName,
            SchemaProperty p1,
            SchemaProperty p2)
        {
            var combineResult = TryCombine(propertyName, nameof(Type), p1.Type, p2.Type)
                .Compose(
                    () => TryCombine(propertyName, nameof(EnumType), p1.EnumType, p2.EnumType),
                    () => TryCombine(
                        propertyName,
                        nameof(DateOutputFormat),
                        p1.DateOutputFormat,
                        p2.DateOutputFormat
                    ),
                    () => TryCombine(propertyName, nameof(Regex), p1.Regex, p2.Regex),
                    () => TryCombine(
                        propertyName,
                        nameof(ErrorBehavior),
                        p1.ErrorBehavior,
                        p2.ErrorBehavior
                    )
                );

            if (combineResult.IsFailure)
                return combineResult.ConvertFailure<SchemaProperty>();

            return new SchemaProperty
            {
                Type             = combineResult.Value.Item1,
                EnumType         = combineResult.Value.Item2,
                DateOutputFormat = combineResult.Value.Item3,
                Regex            = combineResult.Value.Item4,
                ErrorBehavior    = combineResult.Value.Item5,
                Multiplicity     = CombineMultiplicity(p1.Multiplicity, p2.Multiplicity),
                DateInputFormats = CombineLists(p1.DateInputFormats, p2.DateInputFormats),
                Values           = CombineLists(p1.Values,           p2.Values)
            };

            static Result<T, IErrorBuilder> TryCombine<T>(
                string propertyName,
                string propertyPropertyName,
                T t1,
                T t2)
            {
                if (t2 is null)
                    return t1;

                if (t1 is null)
                    return default(T)!;

                if (t1.Equals(t2))
                    return t1;

                return ErrorCode.CannotCombineSchemaProperties.ToErrorBuilder(
                    propertyName,
                    propertyPropertyName,
                    t1,
                    t2
                );
            }

            static IReadOnlyList<string>? CombineLists(
                IReadOnlyList<string>? l1,
                IReadOnlyList<string>? l2)
            {
                if (l1 is null)
                    return l2;

                if (l2 is null)
                    return l1;

                return l1.Concat(l2).Distinct().ToList();
            }

            static Multiplicity CombineMultiplicity(Multiplicity m1, Multiplicity m2)
            {
                if (m1 == m2)
                    return m1;

                if (m1 == Multiplicity.Any || m2 == Multiplicity.Any)
                    return Multiplicity.Any;

                if ((m1 == Multiplicity.ExactlyOne && m2 == Multiplicity.UpToOne)
                 || (m2 == Multiplicity.ExactlyOne && m1 == Multiplicity.UpToOne)
                )
                    return Multiplicity.UpToOne;

                if ((m1 == Multiplicity.ExactlyOne && m2 == Multiplicity.AtLeastOne)
                 || (m2 == Multiplicity.ExactlyOne && m1 == Multiplicity.AtLeastOne)
                )
                    return Multiplicity.AtLeastOne;

                return Multiplicity.Any;
            }
        }
    }
}

}
