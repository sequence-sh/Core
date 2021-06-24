using System.Collections.Generic;
using System.Runtime.Serialization;
using Reductech.EDR.Core.Enums;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// A single property in a a schema.
/// </summary>
[DataContract]
public sealed class SchemaProperty
{
    /// <summary>
    /// The type of the property.
    /// </summary>
    [property: DataMember]
    public SCLType Type { get; set; }

    /// <summary>
    /// If this is an enum, the name of the enum
    /// </summary>
    [property: DataMember]
    public string? EnumType { get; set; }

    /// <summary>
    /// The multiplicity of the property.
    /// </summary>
    [property: DataMember]
    public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

    /// <summary>
    /// If this is an enum, the allowed values.
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<string>? Values { get; set; }

    /// <summary>
    /// The allowed formats for the date
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<string>? DateInputFormats { get; set; }

    /// <summary>
    /// The output format for the date
    /// </summary>
    [property: DataMember]
    public string? DateOutputFormat { get; set; }

    /// <summary>
    /// A regex to validate the string form of the field value
    /// </summary>
    [property: DataMember]
    public string? Regex { get; set; }

    /// <summary>
    /// The error behavior, overriding the default value of the schema.
    /// </summary>
    [property: DataMember]
    public ErrorBehavior? ErrorBehavior { get; set; }
}

}
