using System;
using System.Collections.Generic;
using System.Linq;
using OneOf;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error builder
/// </summary>
public class ErrorBuilder : IErrorBuilder, IEquatable<IErrorBuilder>
{
    /// <summary>
    /// Create a new SingleErrorBuilder
    /// </summary>
    public ErrorBuilder(ErrorCode errorCode, params object?[] data)
    {
        ErrorCode = errorCode;
        Data      = data;
        Timestamp = DateTime.Now;
    }

    /// <summary>
    /// Create a new SingleErrorBuilder
    /// </summary>
    public ErrorBuilder(Exception exception, ErrorCode errorCode)
    {
        ErrorCode = errorCode;
        Data      = exception;
        Timestamp = DateTime.Now;
    }

    /// <inheritdoc />
    public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

    /// <inheritdoc />
    public IEnumerable<ErrorBuilder> GetErrorBuilders()
    {
        yield return this;
    }

    /// <summary>
    /// The time the error was created.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// The Error Code
    /// </summary>
    public ErrorCode ErrorCode { get; }

    /// <summary>
    /// The data
    /// </summary>
    public OneOf<Exception, object?[]> Data { get; }

    /// <summary>
    /// Returns a SingleError with the given location.
    /// </summary>
    public SingleError WithLocationSingle(IErrorLocation location) =>
        new SingleError(location, this);

    /// <inheritdoc />
    public string AsString => Data.Match(
        x => x.Message,
        args => ErrorCode.GetFormattedMessage(args)
    );

    /// <summary>
    /// Equals method
    /// </summary>
    protected bool Equals(ErrorBuilder other) => AsString == other.AsString;

    /// <summary>
    /// Equals method
    /// </summary>
    public bool Equals(IErrorBuilder? errorBuilder)
    {
        if (errorBuilder is ErrorBuilder seb)
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

        if (obj is ErrorBuilder seb)
            return Equals(seb);

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => AsString.GetHashCode();

    /// <summary>
    /// Equals operator
    /// </summary>
    public static bool operator ==(ErrorBuilder? left, ErrorBuilder? right) => Equals(left, right);

    /// <summary>
    /// NotEquals operator
    /// </summary>
    public static bool operator !=(ErrorBuilder? left, ErrorBuilder? right) => !Equals(left, right);
}

}
