﻿using System.Globalization;

namespace Sequence.Core.Entities.Schema;

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
    public override bool IsSuperset(StringFormat other)
    {
        return other is DateTimeStringFormat;
    }

    /// <inheritdoc />
    public override TypeReference GetTypeReference(StringRestrictions restrictions)
    {
        return TypeReference.Actual.Date;
    }

    /// <inheritdoc />
    public override Result<Maybe<ISCLObject>, IErrorBuilder> TryTransform(
        string propertyName,
        ISCLObject entityValue,
        TransformSettings transformSettings,
        TransformRoot transformRoot)
    {
        if (entityValue is SCLDateTime)
        {
            return Maybe<ISCLObject>.None;
        }

        var primitive = entityValue.Serialize(SerializeOptions.Primitive);

        if (DateTime.TryParse(primitive, out var dt1))
        {
            return Maybe<ISCLObject>.From(new SCLDateTime(dt1));
        }

        var formats = transformSettings.DateFormatter.GetFormats(propertyName).ToArray();

        if (!formats.Any())
            formats = null;

        if (DateTime.TryParseExact(primitive, formats, null, DateTimeStyles.None, out var dt2))
        {
            return Maybe<ISCLObject>.From(new SCLDateTime(dt2));
        }

        return ErrorCode.SchemaViolated.ToErrorBuilder(
            $"Should be DateTime",
            propertyName,
            transformRoot.RowNumber,
            transformRoot.Entity
        );
    }

    /// <inheritdoc />
    public override void SetBuilder(JsonSchemaBuilder builder)
    {
        builder.Format(new Format("date-time"));
    }
}
