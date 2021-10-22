using System;
using System.Globalization;
using System.Linq;
using CSharpFunctionalExtensions;
using Json.Schema;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// The date-time string format format
/// </summary>
public record DateTimeStringFormat : StringFormat
{
    private DateTimeStringFormat() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static DateTimeStringFormat Instance { get; } = new();

    /// <inheritdoc />
    public override Result<Maybe<EntityValue>, IErrorBuilder> TryTransform(
        string propertyName,
        EntityValue entityValue,
        TransformSettings transformSettings)
    {
        if (entityValue is EntityValue.DateTime)
        {
            return Maybe<EntityValue>.None;
        }

        var primitive = entityValue.GetPrimitiveString();

        if (DateTime.TryParse(primitive, out var dt1))
        {
            return Maybe<EntityValue>.From(new EntityValue.DateTime(dt1));
        }

        var formats = transformSettings.DateFormatter.GetFormats(propertyName).ToArray();

        if (!formats.Any())
            formats = null;

        if (DateTime.TryParseExact(primitive, formats, null, DateTimeStyles.None, out var dt2))
        {
            return Maybe<EntityValue>.From(new EntityValue.DateTime(dt2));
        }

        return ErrorCode.SchemaViolation.ToErrorBuilder(
            $"Should be DateTime",
            propertyName
        );
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        builder.Format(new Format("date-time"));
    }
}

}
