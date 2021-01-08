using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// A single error
/// </summary>
public abstract class SingleError<TCode> : SingleError where TCode : Enum
{
    /// <summary>
    /// Create a new error with an exception.
    /// </summary>
    protected SingleError(
        IErrorLocation location,
        SingleErrorBuilder<TCode> singleErrorBuilder) : base(
        location,
        singleErrorBuilder
    ) => ErrorCode = singleErrorBuilder.ErrorCode;

    /// <summary>
    /// The error code.
    /// </summary>
    public TCode ErrorCode { get; }

    /// <inheritdoc />
    public override string ErrorCodeString => ErrorCode.ToString();

    /// <summary>
    /// Supports localization of the Error Messages
    /// </summary>
    public abstract IErrorCodeHelper<TCode> ErrorCodeHelper { get; }
}

/// <summary>
/// An single error.
/// </summary>
public abstract class SingleError : IError
{
    /// <summary>
    /// Create a new SingleError. Sets the timestamp to now.
    /// </summary>
    protected SingleError(IErrorLocation location, SingleErrorBuilder errorBuilder)
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
    public SingleErrorBuilder ErrorBuilder { get; }

    /// <summary>
    /// Associated Exception if there is one
    /// </summary>
    public Exception? Exception => ErrorBuilder.Data.TryPickT0(out var e, out _) ? e : null;

    /// <summary>
    /// The error code as a string.
    /// </summary>
    public abstract string ErrorCodeString { get; }

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
    public override string ToString() =>
        ErrorCodeString + ": " + AsString + " in " + Location.AsString;

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
