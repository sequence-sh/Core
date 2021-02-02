using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// Converts Entities to Json
/// </summary>
public class EntityJsonConverter : JsonConverter
{
    private EntityJsonConverter() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static EntityJsonConverter Instance { get; } = new();

    /// <inheritdoc />
    public override void WriteJson(
        JsonWriter writer,
        object? entityObject,
        JsonSerializer serializer)
    {
        if (entityObject is not Entity entity)
            return;

        var dictionary = new Dictionary<string, object?>();

        foreach (var entityProperty in entity)
        {
            var value = GetObject(entityProperty.BestValue);
            dictionary.Add(entityProperty.Name, value);
        }

        serializer.Serialize(writer, dictionary);

        static object? GetObject(EntityValue ev)
        {
            return ev.Match(
                _ => null as object,
                x => x,
                x => x,
                x => x,
                x => x,
                x => x.Value,
                x => x,
                x => x,
                x => x.Select(GetObject).ToList()
            );
        }
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        var objectDict = serializer.Deserialize<Dictionary<string, object>>(reader);

        var entity =
            Entity.Create(objectDict!.Select(x => (new EntityPropertyKey(x.Key), x.Value))!);

        return entity;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => objectType == typeof(Entity);
}

}
