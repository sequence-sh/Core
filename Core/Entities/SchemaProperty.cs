using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Reductech.EDR.Core.Enums;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// A single property in a a schema.
/// </summary>
[Serializable]
public sealed class SchemaProperty
{
    /// <summary>
    /// The type of the property.
    /// </summary>
    [JsonProperty]
    public SCLType Type { get; set; }

    /// <summary>
    /// If this is an enum, the name of the enum
    /// </summary>
    [JsonProperty]
    public string? EnumType { get; set; }

    /// <summary>
    /// The multiplicity of the property.
    /// </summary>
    [JsonProperty]
    public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

    /// <summary>
    /// If this is an enum, the allowed values.
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<string>? Values { get; set; }

    /// <summary>
    /// The allowed formats for the date
    /// </summary>
    [JsonProperty]
    public IReadOnlyList<string>? DateInputFormats { get; set; }

    /// <summary>
    /// The output format for the date
    /// </summary>
    [JsonProperty]
    public string? DateOutputFormat { get; set; }

    /// <summary>
    /// A regex to validate the string form of the field value
    /// </summary>
    [JsonProperty]
    public string? Regex { get; set; }

    /// <summary>
    /// The error behavior, overriding the default value of the schema.
    /// </summary>
    [JsonProperty]
    public ErrorBehavior? ErrorBehavior { get; set; }
}

}
