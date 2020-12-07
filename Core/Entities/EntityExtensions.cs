using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// Methods for creating objects of particular types from entities
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Try to set a string property from an entity
        /// </summary>
        public static Result<Unit, IErrorBuilder> TrySetString(this Entity entity,
            string propertyName,
            string className,
            Action<string> setString)
        {
            if (entity.TryGetValue(propertyName, out var ev) && ev!.Value.IsT1)
            {
                var s = ev.Value.AsT1.Original;
                setString(s);
                return Unit.Default;
            }

            var errorBuilder = ErrorHelper.MissingParameterError(propertyName, className);

            return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
        }

        /// <summary>
        /// Try to set a string list property from an entity
        /// </summary>
        public static Result<Unit, IErrorBuilder> TrySetStringList(this Entity entity,
            string propertyName,
            string className,
            Action<List<string>> setStringList)
        {
            if (entity.TryGetValue(propertyName, out var ev) && ev!.Value.IsT2)
            {
                var l = ev.Value.AsT2.Select(x=>x.Original).ToList();
                setStringList(l);
                return Unit.Default;
            }

            var errorBuilder = ErrorHelper.MissingParameterError(propertyName, className);

            return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
        }

        /// <summary>
        /// Try to set a dictionary property from an entity
        /// </summary>
        public static Result<Unit, IErrorBuilder> TrySetDictionary<T>(this Entity entity,
            string propertyName,
            string className,
            Func<EntityValue, Result<T, IErrorBuilder>> getElement,
            Action<Dictionary<string, T>> setStringDictionary)
        {
            if (entity.TryGetValue(propertyName, out var ev) && ev!.Value.IsT1 && ev!.Value.AsT1.Value.IsT6)
            {
                var topEntity = ev!.Value.AsT1.Value.AsT6;

                var errors = new List<IErrorBuilder>();
                var dict = new Dictionary<string, T>();

                foreach (var (key, value) in topEntity)
                {
                    var element = getElement(value);
                    if(element.IsFailure) errors.Add(element.Error);
                    else dict.Add(key, element.Value);
                }

                if (errors.Any())
                    return Result.Failure<Unit, IErrorBuilder>(ErrorBuilderList.Combine(errors));


                setStringDictionary(dict);
                return Unit.Default;
            }

            var errorBuilder = ErrorHelper.MissingParameterError(propertyName, className);

            return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
        }


        /// <summary>
        /// Try to set a string property from an entity
        /// </summary>
        public static Result<Unit, IErrorBuilder> TrySetBoolean(this Entity entity,
            string propertyName,
            string className,
            Action<bool> setBool)
        {
            if (entity.TryGetValue(propertyName, out var ev) && ev!.Value.IsT1)
            {
                var convertResult = ev.Value.AsT1.TryConvert(new SchemaProperty() {Type = SchemaPropertyType.Bool});
                if (convertResult.IsFailure) return convertResult.ConvertFailure<Unit>();

                setBool(convertResult.Value.Value.AsT3);
                return Unit.Default;
            }

            var errorBuilder = ErrorHelper.MissingParameterError(propertyName, className);

            return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
        }


        /// <summary>
        /// Try to set an enum property from an entity
        /// </summary>
        public static Result<Unit, IErrorBuilder> TrySetEnum<T>(this Entity entity,
            string propertyName,
            string className,
            Action<T> setEnum) where T : struct, Enum
        {
            if (entity.TryGetValue(propertyName, out var ev) && ev!.Value.IsT1)
            {
                if (Enum.TryParse(ev.Value.AsT1.Original, true, out T t))
                {
                    setEnum(t);
                    return Unit.Default;
                }
                else
                {
                    return new ErrorBuilder($" '{typeof(T).Name}.{ev.Value.AsT1.Original}' does not exist", ErrorCode.UnexpectedEnumValue);
                }
            }

            var errorBuilder = ErrorHelper.MissingParameterError(propertyName, className);

            return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
        }

    }
}
