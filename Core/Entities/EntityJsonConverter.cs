using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Reductech.EDR.Core.Entities
{

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
                var value = GetObject(entityProperty.BestValue);
                dictionary.Add(entityProperty.Name, value);
            }

            return dictionary;

            static object? GetObject(EntityValue ev)
            {
                return ev switch
                {
                    EntityValue.NestedEntity nestedEntity => CreateDictionary(nestedEntity.Value),
                    EntityValue.EnumerationValue enumerationValue => enumerationValue.Value.Value,
                    EntityValue.NestedList list => list.Value.Select(GetObject).ToList(),
                    EntityValue.Null nullValue => null,
                    _ => ev.ObjectValue
                };
            }
        }
    }
}

}
