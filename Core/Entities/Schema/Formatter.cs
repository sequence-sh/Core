using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using OneOf;

namespace Reductech.EDR.Core.Entities.Schema
{

/// <summary>
/// Used for formatting entity values
/// </summary>
public class Formatter : OneOfBase<IReadOnlyList<string>,
    IReadOnlyDictionary<string, IReadOnlyList<string>>>
{
    /// <summary>
    /// GVets possible formats for a property
    /// </summary>
    public IEnumerable<string> GetFormats(string propertyName)
    {
        if (!TryPickT0(out var list, out var dict))
        {
            if (!dict.TryGetValue(propertyName, out var list2))
            {
                yield break;
            }

            list = list2;
        }

        foreach (var format in list)
        {
            yield return format;
        }
    }

    public bool IsMatch(string value, string path, bool caseSensitive)
    {
        var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

        if (TryPickT0(out var list, out var dict))
        {
            if (list.Contains(value, comparer))
                return true;
        }
        else if (dict.TryGetValue(path, out var newList))
            if (newList.Contains(value, comparer))
                return true;

        return false;
    }

    /// <summary>
    /// Create a new Formatter
    /// </summary>
    public static Formatter Create(Maybe<OneOf<string, IReadOnlyList<string>, Entity>> dataMaybe)
    {
        if (dataMaybe.HasNoValue)
            return Empty;

        if (dataMaybe.GetValueOrThrow().TryPickT0(out var s, out var r1))
        {
            return new Formatter(new List<string>() { s });
        }

        if (r1.TryPickT0(out var list, out var entity))
            return new Formatter(
                OneOf<IReadOnlyList<string>, IReadOnlyDictionary<string, IReadOnlyList<string>>>
                    .FromT0(list)
            );

        var dict = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in entity)
        {
            if (prop.Value is EntityValue.NestedList nl)
                dict.Add(prop.Name, nl.Value.Select(x => x.GetPrimitiveString()).ToList());
            else
                dict.Add(prop.Name, new List<string>() { prop.Value.GetPrimitiveString() });
        }

        return new Formatter(dict);
    }

    /// <summary>
    /// Empty conversion mode
    /// </summary>
    public static Formatter Empty { get; } = new(new List<string>());

    /// <inheritdoc />
    protected Formatter(
        OneOf<IReadOnlyList<string>, IReadOnlyDictionary<string, IReadOnlyList<string>>> input) :
        base(input) { }
}

}
