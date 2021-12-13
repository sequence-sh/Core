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
        var dictionary = CreateDictionary(value);

        JsonSerializer.Serialize(writer, dictionary, options);

        static Dictionary<string, object?> CreateDictionary(Entity entity)
        {
            var dictionary = new Dictionary<string, object?>();

            foreach (var entityProperty in entity)
            {
                var value = GetObject(entityProperty.Value);
                dictionary.Add(entityProperty.Name, value);
            }

            return dictionary;

            static object? GetObject(ISCLObject ev)
            {
                return ev switch
                {
                    ISCLObject.NestedEntity nestedEntity => CreateDictionary(nestedEntity.Value),
                    ISCLObject.EnumerationValue enumerationValue => enumerationValue.Value
                        .EnumValue,
                    ISCLObject.NestedList list => list.Value.Select(GetObject).ToList(),
                    ISCLObject.Null _          => null,
                    _                          => ev.ObjectValue
                };
            }
        }
    }
}
