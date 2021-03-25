using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core
{

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
            var json = JsonConvert.SerializeObject(
                entity,
                Formatting.None,
                EntityJsonConverter.Instance
            );

            var schema = JsonConvert.DeserializeObject<T>(json);
            return schema;
        }
        catch (Exception e)
        {
            return ErrorCode.CannotConvertNestedEntity.ToErrorBuilder(e);
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
        var properties = obj.GetType().GetProperties();

        var props = new List<EntityProperty>();

        var i = 0;

        foreach (var propertyInfo in properties)
        {
            var value = propertyInfo.GetValue(obj);

            if (value is not null)
            {
                var ev = EntityValue.CreateFromObject(value);
                var ep = new EntityProperty(propertyInfo.Name, ev, null, i);
                props.Add(ep);
            }

            i++;
        }

        return new Entity(props);
    }
}

}
