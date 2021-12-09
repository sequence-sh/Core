using System;
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Core.Internal.Errors;

/// <summary>
/// An single error.
/// </summary>
public record SingleError(ErrorLocation Location, ErrorBuilder ErrorBuilder) : IError
{
    /// <summary>
    /// Create a new SingleError.
    /// </summary>
    public SingleError(ErrorLocation location, Exception exception, ErrorCodeBase errorCodeBase)
        : this(location, new ErrorBuilder(exception, errorCodeBase)) { }

    /// <summary>
    /// Create a new SingleError.
    /// </summary>
    public SingleError(ErrorLocation location, ErrorCodeBase errorCodeBase, params object?[] args)
        : this(location, new ErrorBuilder(errorCodeBase, args)) { }

    /// <summary>
    /// Error Message String.
    /// </summary>
    public string Message => ErrorBuilder.AsString;

    /// <summary>
    /// Associated Exception if there is one
    /// </summary>
    public Exception? Exception =>
        ErrorBuilder.Data is ErrorData.ExceptionData ed ? ed.Exception : null;

    /// <inheritdoc />
    public IEnumerable<SingleError> GetAllErrors()
    {
        yield return this;
    }

    /// <inheritdoc />
    public string AsString => Message;

    /// <inheritdoc />
    public string AsStringWithLocation => $"{Message} in {Location}";

    /// <inheritdoc />
    public IErrorBuilder ToErrorBuilder => ErrorBuilder;

    /// <inheritdoc />
    public override string ToString() => Message + " in " + Location.AsString();

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
}
