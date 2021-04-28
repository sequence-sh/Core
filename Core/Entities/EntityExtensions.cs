using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// Methods for creating objects of particular types from entities
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string>
        TryGetNestedString(
            this Entity current,
            params string[] properties)
    {
        if (!properties.Any())
            return Maybe<string>.None;

        var nested = TryGetNestedEntity(current, properties[..^1]);

        if (nested.HasNoValue)
            return Maybe<string>.None;

        var lastProp = nested.Value.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string>.None;

        return lastProp.Value.GetPrimitiveString();
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
    /// Tries to get a nested entity. Returns empty if that property is not found
    /// </summary>
    public static Maybe<Entity> TryGetNestedEntity(this Entity current, params string[] properties)
    {
        if (!properties.Any())
            return current;

        foreach (var property in properties)
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<Entity>.None;

            if (v.Value is EntityValue.NestedEntity ne)
                current = ne.Value;
            else
                return Maybe<Entity>.None;
        }

        return current;
    }

    /// <summary>
    /// Tries to get a nested string.
    /// </summary>
    public static Maybe<string[]>
        TryGetNestedList(
            this Entity current,
            params string[] properties)
    {
        if (!properties.Any())
            return Maybe<string[]>.None;

        var nested = TryGetNestedEntity(current, properties[..^1]);

        if (nested.HasNoValue)
            return Maybe<string[]>.None;

        var lastProp = nested.Value.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string[]>.None;

        if (lastProp.Value is not EntityValue.NestedList list)
            return Maybe<string[]>.None;

        var stringArray = list.Value.Select(x => x.GetPrimitiveString()).ToArray();
        return stringArray;
    }
}

}
