using System;
using System.Collections.Generic;
using System.Linq;
using OneOf;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error builder
/// </summary>
public abstract class SingleErrorBuilder : IErrorBuilder, IEquatable<IErrorBuilder>
{
    /// <summary>
    /// Create a new SingleErrorBuilder
    /// </summary>
    protected SingleErrorBuilder(OneOf<Exception, object?[]> data)
    {
        Data      = data;
        Timestamp = DateTime.Now;
    }

    /// <inheritdoc />
    public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

    /// <inheritdoc />
    public IEnumerable<SingleErrorBuilder> GetErrorBuilders()
    {
        yield return this;
    }

    /// <summary>
    /// The time the error was created.
    /// </summary>
    public DateTime Timestamp { get; }

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

    /// <summary>
    /// Equals method
    /// </summary>
    protected bool Equals(SingleErrorBuilder other) => AsString == other.AsString;

    /// <summary>
    /// Equals method
    /// </summary>
    public bool Equals(IErrorBuilder? errorBuilder)
    {
        if (errorBuilder is SingleErrorBuilder seb)
            return Equals(seb);

        if (errorBuilder is ErrorBuilderList ebl && ebl.ErrorBuilders.Count == 1)
            return Equals(ebl.ErrorBuilders.Single());

        return false;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is SingleErrorBuilder seb)
            return Equals(seb);

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => AsString.GetHashCode();

    /// <summary>
    /// Equals operator
    /// </summary>
    public static bool operator ==(SingleErrorBuilder? left, SingleErrorBuilder? right) =>
        Equals(left, right);

    /// <summary>
    /// NotEquals operator
    /// </summary>
    public static bool operator !=(SingleErrorBuilder? left, SingleErrorBuilder? right) =>
        !Equals(left, right);
}

}
