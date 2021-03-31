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

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string>.None;

            if (v.Value is EntityValue.NestedEntity ne)
                current = ne.Value;
            else
                return Maybe<string>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string>.None;

        return lastProp.Value.GetPrimitiveString() ?? "";
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
            params string[] properties)
    {
        if (!properties.Any())
            return Maybe<string[]>.None;

        foreach (var property in properties.SkipLast(1))
        {
            var v = current.TryGetValue(property);

            if (v.HasNoValue)
                return Maybe<string[]>.None;

            if (v.Value is EntityValue.NestedEntity ne)
                current = ne.Value;
            else
                return Maybe<string[]>.None;
        }

        var lastProp = current.TryGetValue(properties.Last());

        if (lastProp.HasNoValue)
            return Maybe<string[]>.None;

        if (lastProp.Value is not EntityValue.NestedList list)
            return Maybe<string[]>.None;

        var stringArray = Enumerable.ToArray(list.Value.Select(x => x.GetPrimitiveString() ?? ""));
        return stringArray;
    }
}

}
