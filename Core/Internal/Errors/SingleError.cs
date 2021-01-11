using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// An single error.
/// </summary>
public class SingleError : IError
{
    /// <summary>
    /// Create a new SingleError.
    /// </summary>
    public SingleError(IErrorLocation location, Exception exception, ErrorCodeBase errorCodeBase)
    {
        Location     = location;
        ErrorBuilder = new ErrorBuilder(exception, errorCodeBase);
    }

    /// <summary>
    /// Create a new SingleError.
    /// </summary>
    public SingleError(IErrorLocation location, ErrorCodeBase errorCodeBase, params object?[] args)
    {
        Location     = location;
        ErrorBuilder = new ErrorBuilder(errorCodeBase, args);
    }

    /// <summary>
    /// Create a new SingleError.
    /// </summary>
    public SingleError(IErrorLocation location, ErrorBuilder errorBuilder)
    {
        Location     = location;
        ErrorBuilder = errorBuilder;
    }

    /// <summary>
    /// Error Message String.
    /// </summary>
    public string Message => ErrorBuilder.AsString;

    /// <summary>
    /// The location where this error arose. This could be a line number.
    /// </summary>
    public IErrorLocation Location { get; }

    /// <summary>
    /// The ErrorBuilder
    /// </summary>
    public ErrorBuilder ErrorBuilder { get; }

    /// <summary>
    /// Associated Exception if there is one
    /// </summary>
    public Exception? Exception => ErrorBuilder.Data.TryPickT0(out var e, out _) ? e : null;

    /// <inheritdoc />
    public IEnumerable<SingleError> GetAllErrors()
    {
        yield return this;
    }

    /// <inheritdoc />
    public string AsString => Message;

    /// <inheritdoc />
    public IErrorBuilder ToErrorBuilder => ErrorBuilder;

    /// <inheritdoc />
    public override string ToString() => Message + " in " + Location.AsString;

    /// <inheritdoc />
    public bool Equals(IError? other)
    {
        return other switch
        {
            ErrorList errorList => errorList.GetAllErrors().Count() == 1 &&
                                   Equals(errorList.GetAllErrors().Single()),
            SingleError singleError => Equals(singleError),
            _                       => false
        };
    }

    private bool Equals(SingleError other)
    {
        return ErrorBuilder == other.ErrorBuilder &&
               Location.Equals(other.Location);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is IError error && Equals(error);
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(ErrorBuilder, Location);
}

}
