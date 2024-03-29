﻿namespace Sequence.Core.Entities.Schema;

/// <summary>
/// Used for formatting entity values
/// </summary>
public class Formatter : OneOfBase<IReadOnlyList<string>,
    IReadOnlyDictionary<string, IReadOnlyList<string>>>
{
    /// <summary>
    /// Gets possible formats for a property
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

    /// <summary>
    /// Whether a particular value is a match
    /// </summary>
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
            if (prop.Value is IArray nl)
                dict.Add(
                    prop.Key.Inner,
                    nl.ListIfEvaluated()
                        .Value.Select(x => x.Serialize(SerializeOptions.Primitive))
                        .ToList()
                );
            else
                dict.Add(
                    prop.Key.Inner,
                    new List<string>() { prop.Value.Serialize(SerializeOptions.Primitive) }
                );
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
