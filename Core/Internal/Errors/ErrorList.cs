using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// Multiple errors.
/// </summary>
public class ErrorList : IError
{
    /// <summary>
    /// Create a new ErrorList
    /// </summary>
    /// <param name="allErrors"></param>
    public ErrorList(IReadOnlyCollection<SingleError> allErrors) => _allErrors = allErrors;

    private readonly IReadOnlyCollection<SingleError> _allErrors;

    /// <inheritdoc />
    public IEnumerable<SingleError> GetAllErrors() => _allErrors;

    /// <inheritdoc />
    public string AsString => string.Join("; ", _allErrors.Select(x => x.AsString));

    /// <summary>
    /// Combine multiple run errors.
    /// </summary>
    public static IError Combine(IEnumerable<IError> source)
    {
        var errors = source.SelectMany(x => x.GetAllErrors()).ToList();

        if (errors.Count == 1)
            return errors.Single();

        return new ErrorList(errors);
    }

    /// <inheritdoc />
    public bool Equals(IError? other) =>
        other != null && _allErrors.SequenceEqual(other.GetAllErrors());

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is IError e && Equals(e);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(
        _allErrors.First().GetHashCode(),
        _allErrors.Count
    );
}

}
