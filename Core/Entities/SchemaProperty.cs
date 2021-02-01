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
    /// The format strings.
    /// For Date, this will contain possible date formats.
    /// For Enum, this will contain possible enum values.
    /// </summary>
    [ConfigProperty(4)]
    public IReadOnlyList<string>? Format { get; set; }

    /// <summary>
    /// A regex to validate the string form of the field value
    /// </summary>
    [ConfigProperty(5)]
    public string? Regex { get; set; }

    /// <summary>
    /// The error behaviour, overriding the default value of the schema.
    /// </summary>
    [ConfigProperty(6)]
    public ErrorBehaviour? ErrorBehaviour { get; set; }

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
            entity.TrySetEnum<ErrorBehaviour>(
                true,
                nameof(ErrorBehaviour),
                e => schemaProperty.ErrorBehaviour = e
            )
        );

        results.Add(
            entity.TrySetStringList(
                true,
                nameof(Format),
                s => schemaProperty.Format = s
            )
        );

        results.Add(
            entity.TrySetString(
                true,
                nameof(Regex),
                s => schemaProperty.Regex = s
            )
        ); //Ignore the result of this

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
            (nameof(Format), EntityValue.CreateFromObject(Format)),
            (nameof(Regex), EntityValue.CreateFromObject(Regex)),
            (nameof(ErrorBehaviour), EntityValue.CreateFromObject(ErrorBehaviour)),
        }.Select((x, i) => new EntityProperty(x.Item1, x.Item2, null, i));

        var entity = new Entity(schemaProperties);
        return entity;
    }
}

}
