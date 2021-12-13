using System.Globalization;

namespace Reductech.EDR.Core.Entities.Schema;

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
    public override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform(
        string propertyName,
        ISCLObject entityValue,
        TransformSettings transformSettings)
    {
        if (entityValue is ISCLObject.DateTime)
        {
            return Maybe<ISCLObject>.None;
        }

        var primitive = entityValue.GetPrimitiveString();

        if (DateTime.TryParse(primitive, out var dt1))
        {
            return Maybe<ISCLObject>.From(new ISCLObject.DateTime(dt1));
        }

        var formats = transformSettings.DateFormatter.GetFormats(propertyName).ToArray();

        if (!formats.Any())
            formats = null;

        if (DateTime.TryParseExact(primitive, formats, null, DateTimeStyles.None, out var dt2))
        {
            return Maybe<ISCLObject>.From(new ISCLObject.DateTime(dt2));
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
