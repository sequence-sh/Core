﻿using System.Text.Json.Serialization;

namespace Sequence.Core;

/// <summary>
/// An object which can be converted to and from an entity
/// </summary>
public interface IEntityConvertible { }

/// <summary>
/// Methods to convert between IEntityConvertible and Entity
/// </summary>
public static class EntityConversionHelpers
{
    /// <summary>
    /// Tries to create an object from an entity.
    /// Ignores unexpected properties.
    /// </summary>
    public static Result<T, IErrorBuilder> TryCreateFromEntity<T>(Entity entity)
    {
        try
        {
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter(), VersionJsonConverter.Instance },
                PropertyNameCaseInsensitive = true
            };

            var entityJson = JsonSerializer.Serialize(entity, options);
            var obj        = JsonSerializer.Deserialize<T>(entityJson, options);

            if (obj is null)
                return ErrorCode.CouldNotParse.ToErrorBuilder(entityJson, typeof(T).Name);

            return obj;
        }
        catch (Exception e)
        {
            return ErrorCode.CouldNotParse.ToErrorBuilder(e);
        }
    }

    /// <summary>
    /// Convert an object to an entity
    /// </summary>
    public static Entity ConvertToEntity(this IEntityConvertible obj)
    {
        return ConvertToEntity(obj as object);
    }

    /// <summary>
    /// Convert an object to an entity
    /// </summary>
    public static Entity ConvertToEntity(object obj)
    {
        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var keys   = ImmutableArray.CreateBuilder<EntityKey>();
        var values = ImmutableArray.CreateBuilder<ISCLObject>();

        foreach (var propertyInfo in properties)
        {
            var value = propertyInfo.GetValue(obj);

            if (value is not null)
            {
                keys.Add(new EntityKey(propertyInfo.Name));
                var ev = ISCLObject.CreateFromCSharpObject(value);
                values.Add(ev);
            }
        }

        return new Entity(keys.ToImmutable(), values.ToImmutable());
    }
}
