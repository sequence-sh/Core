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
    public static Result<Unit, IErrorBuilder> TrySetString(
        this Entity entity,
        string propertyName,
        Action<string> setString)
    {
        var value = entity.TryGetValue(propertyName).Map(x => x.GetString());

        if (value.HasValue)
        {
            setString(value.Value);
            return Unit.Default;
        }

        var errorBuilder = ErrorHelper.MissingParameterError(propertyName);

        return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
    }

    /// <summary>
    /// Try to set a string list property from an entity
    /// </summary>
    public static Result<Unit, IErrorBuilder> TrySetStringList(
        this Entity entity,
        string propertyName,
        Action<List<string>> setStringList)
    {
        var value = entity.TryGetValue(propertyName)
            .Bind(x => x.TryGetEntityValueList())
            .Map(x => x.Select(z => z.GetString()).ToList());

        if (value.HasValue)
        {
            setStringList(value.Value);
            return Unit.Default;
        }

        var errorBuilder = ErrorHelper.MissingParameterError(propertyName);

        return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
    }

    /// <summary>
    /// Try to set a dictionary property from an entity
    /// </summary>
    public static Result<Unit, IErrorBuilder> TrySetDictionary<T>(
        this Entity entity,
        string propertyName,
        Func<EntityValue, Result<T, IErrorBuilder>> getElement,
        Action<Dictionary<string, T>> setDictionary)
    {
        var value = entity.TryGetValue(propertyName)
            .Bind(x => x.TryGetEntity());

        if (value.HasValue)
        {
            var errors = new List<IErrorBuilder>();
            var dict   = new Dictionary<string, T>();

            foreach (var property in value.Value)
            {
                var element = getElement(property.BestValue);

                if (element.IsFailure)
                    errors.Add(element.Error);
                else
                    dict.Add(property.Name, element.Value);
            }

            if (errors.Any())
                return Result.Failure<Unit, IErrorBuilder>(ErrorBuilderList.Combine(errors));

            setDictionary(dict);
            return Unit.Default;
        }

        var errorBuilder = ErrorHelper.MissingParameterError(propertyName);

        return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
    }

    /// <summary>
    /// Try to set a string property from an entity
    /// </summary>
    public static Result<Unit, IErrorBuilder> TrySetBoolean(
        this Entity entity,
        string propertyName,
        Action<bool> setBool)
    {
        var v = entity.TryGetValue(propertyName).Bind(x => x.TryGetBool());

        if (v.HasValue)
        {
            setBool(v.Value);
            return Unit.Default;
        }

        var errorBuilder = ErrorHelper.MissingParameterError(propertyName);

        return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
    }

    /// <summary>
    /// Try to set an enum property from an entity
    /// </summary>
    public static Result<Unit, IErrorBuilder> TrySetEnum<T>(
        this Entity entity,
        string propertyName,
        Action<T> setEnum) where T : struct, Enum
    {
        var v = entity.TryGetValue(propertyName)
            .Bind(x => x.TryGetEnumeration(typeof(T).Name, Enum.GetNames<T>()))
            .Bind(x => x.TryConvert<T>());

        if (v.HasValue)
        {
            setEnum(v.Value);
            return Unit.Default;
        }

        var errorBuilder = ErrorHelper.MissingParameterError(propertyName);

        return Result.Failure<Unit, IErrorBuilder>(errorBuilder);
    }

    /// <summary>
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string>
        TryGetNestedString(
            this Entity current,
            params string[] properties) //TODO remove when we can update core
    {
        if (!properties.Any())
            return Maybe<string>.None;

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string>.None;

            if (v.Value.TryPickT7(out var e, out _))
                current = e;
            else
                return Maybe<string>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string>.None;

        return lastProp.Value.ToString();
    }

    /// <summary>
    /// Tries to get a nested boolean. Returns false if that property is not found.
    /// </summary>
    public static bool TryGetNestedBool(this Entity current, params string[] properties)
    {
        var s = TryGetNestedString(current, properties);

        if (s.HasNoValue)
            return false;

        var b = bool.TryParse(s.Value, out var r) && r;

        return b;
    }

    /// <summary>
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string[]>
        TryGetNestedList(
            this Entity current,
            params string[] properties) //TODO remove when we can update core
    {
        if (!properties.Any())
            return Maybe<string[]>.None;

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string[]>.None;

            if (v.Value.TryPickT7(out var e, out _))
                current = e;
            else
                return Maybe<string[]>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string[]>.None;

        if (!lastProp.Value.TryPickT8(out var list, out _))
            return Maybe<string[]>.None;

        var stringArray = list.Select(x => x.ToString()).ToArray();
        return stringArray;
    }
}

}
