using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// More than one errorBuilders
/// </summary>
public class ErrorBuilderList : IErrorBuilder
{
    /// <summary>
    /// Create a new ErrorBuilderList
    /// </summary>
    public ErrorBuilderList(IReadOnlyCollection<ErrorBuilder> errorBuilders) =>
        ErrorBuilders = errorBuilders;

    /// <summary>
    /// The errorBuilders
    /// </summary>
    public IReadOnlyCollection<ErrorBuilder> ErrorBuilders { get; }

    /// <inheritdoc />
    public IError WithLocation(ErrorLocation location) => new ErrorList(
        ErrorBuilders.Select(x => x.WithLocationSingle(location)).ToList()
    );

    /// <inheritdoc />
    public IEnumerable<ErrorBuilder> GetErrorBuilders() => ErrorBuilders;

    /// <inheritdoc />
    public string AsString => string.Join("; ", ErrorBuilders.Select(x => x.AsString));

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <summary>
    /// Combine multiple ErrorBuilders
    /// </summary>
    public static IErrorBuilder Combine(IEnumerable<IErrorBuilder> errorBuilders)
    {
        var errors = errorBuilders.SelectMany(x => x.GetErrorBuilders()).Distinct().ToList();

        if (errors.Count == 1)
            return errors.Single();

        return new ErrorBuilderList(errors);
    }

    /// <inheritdoc />
    public bool Equals(IErrorBuilder? other)
    {
        if (other is null)
            return ErrorBuilders.Count == 0;

        if (ErrorBuilders.Count == 1 && other is ErrorBuilder eb)
        {
            return ErrorBuilders.Single().Equals(eb);
        }

        return GetErrorBuilders().SequenceEqual(other.GetErrorBuilders());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (ErrorBuilders.Count == 0)
            return 0;

        else if (ErrorBuilders.Count == 1)
            return ErrorBuilders.Single().GetHashCode();

        return HashCode.Combine(ErrorBuilders.Count, ErrorBuilders.First(), ErrorBuilders.Last());
    }
}

}
