using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;

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
    /// <param name="properties"></param>
    /// <returns></returns>
    public static Maybe<SchemaProperty> Combine(
        IEnumerable<Maybe<SchemaProperty>> properties,
        int expectedCount)
    {
        throw new NotImplementedException();
    }

    public static Result<Maybe<SchemaProperty>, IErrorBuilder> Combine(
        Maybe<SchemaProperty> property1,
        Maybe<SchemaProperty> property2)
    {
        if (property1.HasValue)
        {
            if (property2.HasValue) { }

            return property1.Map(AllowNone);
        }

        return property2.Map(AllowNone);

        static SchemaProperty AllowNone(SchemaProperty schemaProperty)
        {
            if (schemaProperty.Multiplicity == Multiplicity.ExactlyOne)
                return schemaProperty with { Multiplicity = Multiplicity.UpToOne };

            return schemaProperty;
        }
    }
}

}
