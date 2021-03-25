using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reductech.EDR.Core.Enums;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// A single property in a a schema.
/// </summary>
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

    /// <summary>
    /// Convert this SchemaProperty to an entity
    /// </summary>
    /// <returns></returns>
    public Entity ConvertToEntity()
    {
        var schemaProperties = new[]
        {
            (nameof(Type), EntityValue.CreateFromObject(Type)),
            (nameof(EnumType), EntityValue.CreateFromObject(EnumType)),
            (nameof(Multiplicity), EntityValue.CreateFromObject(Multiplicity)),
            (nameof(Values), EntityValue.CreateFromObject(Values)),
            (nameof(DateInputFormats), EntityValue.CreateFromObject(DateInputFormats)),
            (nameof(DateOutputFormat), EntityValue.CreateFromObject(DateOutputFormat)),
            (nameof(Regex), EntityValue.CreateFromObject(Regex)),
            (nameof(ErrorBehavior), EntityValue.CreateFromObject(ErrorBehavior)),
        }.Select((x, i) => new EntityProperty(x.Item1, x.Item2, null, i));

        var entity = new Entity(schemaProperties);
        return entity;
    }
}

}
