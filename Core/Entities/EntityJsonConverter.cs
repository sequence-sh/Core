using System.Text.Json.Serialization;

namespace Reductech.EDR.Core.Entities;

/// <summary>
/// Converts Entities to and from Json
/// </summary>
public class EntityJsonConverter : JsonConverter<Entity>
{
    /// <summary>
    /// The instance
    /// </summary>
    public static EntityJsonConverter Instance { get; } = new();

    /// <inheritdoc />
    public override Entity Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using JsonDocument document = JsonDocument.ParseValue(ref reader);
        return Entity.Create(document.RootElement.Clone()); //TODO remove the Clone
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Entity value, JsonSerializerOptions options)
    {
        var dictionary = value.ToCSharpObject();

        JsonSerializer.Serialize(writer, dictionary, options);
    }
}
