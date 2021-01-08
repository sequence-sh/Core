using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error builder
/// </summary>
public abstract class SingleErrorBuilder : IErrorBuilder
{
    /// <inheritdoc />
    public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

    /// <inheritdoc />
    public IEnumerable<SingleErrorBuilder> GetErrorBuilders()
    {
        yield return this;
    }

    /// <summary>
    /// Returns a SingleError with the given location.
    /// </summary>
    public abstract SingleError WithLocationSingle(IErrorLocation location);

    /// <inheritdoc />
    public abstract string AsString { get; }
}

}
