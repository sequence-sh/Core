using System.Text.Json.Serialization;

namespace Reductech.Sequence.Core.Entities;

/// <summary>
/// Converts Versions to and from JSON
/// </summary>
public class VersionJsonConverter : JsonConverter<Version>
{
    /// <summary>
    /// The instance
    /// </summary>
    public static JsonConverter<Version> Instance { get; } = new VersionJsonConverter();

    /// <inheritdoc />
    public override Version? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var versionString = reader.GetString();

        if (Version.TryParse(versionString, out var result))
        {
            return result;
        }

        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
