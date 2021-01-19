using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// Extension methods to extract values from entities
/// </summary>
public static class EntityValueExtensions
{
    /// <summary>
    /// Get the string value of this entity value
    /// </summary>
    /// <returns></returns>
    public static string GetString(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => "",
            x => x,
            x => x.ToString(),
            x => x.ToString(Constants.DoubleFormat),
            x => x.ToString(),
            x => x.ToString(),
            x => x.ToString("O"),
            x => x.ToString(),
            x => string.Join(",", x.Select(v => v.GetString()))
        );
    }

    /// <summary>
    /// Try to get an integer value from an entity value
    /// </summary>
    public static Maybe<int> TryGetInt(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<int>.None,
            x => int.TryParse(x, out var i) ? i : Maybe<int>.None,
            i => i,
            _ => Maybe<int>.None,
            _ => Maybe<int>.None,
            _ => Maybe<int>.None,
            _ => Maybe<int>.None,
            _ => Maybe<int>.None,
            _ => Maybe<int>.None
        );
    }

    /// <summary>
    /// Try to get an double value from an entity value
    /// </summary>
    public static Maybe<double> TryGetDouble(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<double>.None,
            x => double.TryParse(x, out var d) ? d : Maybe<double>.None,
            i => i,
            d => d,
            _ => Maybe<double>.None,
            _ => Maybe<double>.None,
            _ => Maybe<double>.None,
            _ => Maybe<double>.None,
            _ => Maybe<double>.None
        );
    }

    /// <summary>
    /// Try to get a boolean value from an entity value
    /// </summary>
    public static Maybe<bool> TryGetBool(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<bool>.None,
            x => bool.TryParse(x, out var b) ? b : Maybe<bool>.None,
            _ => Maybe<bool>.None,
            _ => Maybe<bool>.None,
            x => x,
            _ => Maybe<bool>.None,
            _ => Maybe<bool>.None,
            _ => Maybe<bool>.None,
            _ => Maybe<bool>.None
        );
    }

    /// <summary>
    /// Try to get an enumeration value from an entity value
    /// </summary>
    public static Maybe<Enumeration> TryGetEnumeration(
        this EntityValue entityValue,
        string enumName,
        IEnumerable<string> possibleValues)
    {
        return entityValue.Match(
            _ => Maybe<Enumeration>.None,
            s => possibleValues.Contains(s, StringComparer.OrdinalIgnoreCase)
                ? new Enumeration(enumName, s)
                : Maybe<Enumeration>.None,
            _ => Maybe<Enumeration>.None,
            _ => Maybe<Enumeration>.None,
            _ => Maybe<Enumeration>.None,
            e => e,
            _ => Maybe<Enumeration>.None,
            _ => Maybe<Enumeration>.None,
            _ => Maybe<Enumeration>.None
        );
    }

    /// <summary>
    /// Try to get a dataTime from an entity value
    /// </summary>
    public static Maybe<DateTime> TryGetDateTime(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<DateTime>.None,
            x => DateTime.TryParse(x, out var dt) ? dt : Maybe<DateTime>.None,
            _ => Maybe<DateTime>.None,
            _ => Maybe<DateTime>.None,
            _ => Maybe<DateTime>.None,
            _ => Maybe<DateTime>.None,
            dt => dt,
            _ => Maybe<DateTime>.None,
            _ => Maybe<DateTime>.None
        );
    }

    /// <summary>
    /// Try to get an entity from an entity value
    /// </summary>
    public static Maybe<Entity> TryGetEntity(this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            _ => Maybe<Entity>.None,
            e => e,
            _ => Maybe<Entity>.None
        );
    }

    /// <summary>
    /// Try to get a list value from an entity value
    /// </summary>
    public static Maybe<IReadOnlyList<EntityValue>> TryGetEntityValueList(
        this EntityValue entityValue)
    {
        return entityValue.Match(
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            _ => Maybe<IReadOnlyList<EntityValue>>.None,
            l => l
        );
    }
}

}
