using System;
using System.Collections.Generic;
using OneOf;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error builder
/// </summary>
public abstract class SingleErrorBuilder : IErrorBuilder
{
    /// <summary>
    /// Create a new SingleErrorBuilder
    /// </summary>
    protected SingleErrorBuilder(OneOf<Exception, object?[]> data) => Data = data;

    /// <inheritdoc />
    public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

    /// <inheritdoc />
    public IEnumerable<SingleErrorBuilder> GetErrorBuilders()
    {
        yield return this;
    }

    /// <summary>
    /// The data
    /// </summary>
    public OneOf<Exception, object?[]> Data { get; }

    /// <summary>
    /// Returns a SingleError with the given location.
    /// </summary>
    public abstract SingleError WithLocationSingle(IErrorLocation location);

    /// <inheritdoc />
    public abstract string AsString { get; }
}

}
