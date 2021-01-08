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
    public ErrorBuilderList(IReadOnlyCollection<SingleErrorBuilder> errorBuilders) =>
        ErrorBuilders = errorBuilders;

    /// <summary>
    /// The errorBuilders
    /// </summary>
    public IReadOnlyCollection<SingleErrorBuilder> ErrorBuilders { get; }

    /// <inheritdoc />
    public IError WithLocation(IErrorLocation location) => new ErrorList(
        ErrorBuilders.Select(x => x.WithLocationSingle(location)).ToList()
    );

    /// <inheritdoc />
    public IEnumerable<SingleErrorBuilder> GetErrorBuilders() => ErrorBuilders;

    /// <inheritdoc />
    public string AsString => string.Join("; ", ErrorBuilders.Select(x => x.AsString));

    /// <inheritdoc />
    public override string ToString() => AsString;

    /// <summary>
    /// Combine multiple ErrorBuilders
    /// </summary>
    public static IErrorBuilder Combine(IEnumerable<IErrorBuilder> errorBuilders)
    {
        var errors = errorBuilders.SelectMany(x => x.GetErrorBuilders()).ToList();

        if (errors.Count == 1)
            return errors.Single();

        return new ErrorBuilderList(errors);
    }
}

}
