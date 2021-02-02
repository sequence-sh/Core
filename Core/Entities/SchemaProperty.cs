using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

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
    [ConfigProperty(1)]
    public SchemaPropertyType Type { get; set; }

    /// <summary>
    /// If this is an enum, the name of the enum
    /// </summary>
    [ConfigProperty(2)]
    public string? EnumType { get; set; }

    /// <summary>
    /// The multiplicity of the property.
    /// </summary>
    [ConfigProperty(3)]
    public Multiplicity Multiplicity { get; set; } = Multiplicity.Any;

    /// <summary>
    /// If this is an enum, the allowed values.
    /// </summary>
    [ConfigProperty(4)]
    public IReadOnlyList<string>? Values { get; set; }

    /// <summary>
    /// The allowed formats for the date
    /// </summary>
    [ConfigProperty(5)]
    public IReadOnlyList<string>? DateInputFormats { get; set; }

    /// <summary>
    /// The output format for the date
    /// </summary>
    [ConfigProperty(6)]
    public string? DateOutputFormat { get; set; }

    /// <summary>
    /// A regex to validate the string form of the field value
    /// </summary>
    [ConfigProperty(5)]
    public string? Regex { get; set; }

    /// <summary>
    /// The error behavior, overriding the default value of the schema.
    /// </summary>
    [ConfigProperty(6)]
    public ErrorBehavior? ErrorBehavior { get; set; }

    /// <summary>
    /// Tries to create a schema from an entity.
    /// Ignores unexpected properties.
    /// </summary>
    public static Result<SchemaProperty, IErrorBuilder> TryCreateFromEntity(Entity entity)
    {
        var results        = new List<Result<Unit, IErrorBuilder>>();
        var schemaProperty = new SchemaProperty();

        results.Add(
            entity.TrySetEnum<SchemaPropertyType>(
                false,
                nameof(Type),
                s => schemaProperty.Type = s
            )
        );

        results.Add(
            entity.TrySetString(
                true,
                nameof(EnumType),
                s => schemaProperty.EnumType = s
            )
        );

        results.Add(
            entity.TrySetEnum<Multiplicity>(
                false,
                nameof(Multiplicity),
                s => schemaProperty.Multiplicity = s
            )
        );

        results.Add(
            entity.TrySetEnum<ErrorBehavior>(
                true,
                nameof(ErrorBehavior),
                e => schemaProperty.ErrorBehavior = e
            )
        );

        results.Add(
            entity.TrySetStringList(
                true,
                nameof(Values),
                s => schemaProperty.Values = s
            )
        );

        results.Add(
            entity.TrySetStringList(
                true,
                nameof(DateInputFormats),
                s => schemaProperty.DateInputFormats = s
            )
        );

        results.Add(
            entity.TrySetString(
                true,
                nameof(DateOutputFormat),
                s => schemaProperty.DateOutputFormat = s
            )
        );

        results.Add(
            entity.TrySetString(
                true,
                nameof(Regex),
                s => schemaProperty.Regex = s
            )
        );

        var r = results.Combine(ErrorBuilderList.Combine)
            .Map(_ => schemaProperty);

        return r;
    }

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
