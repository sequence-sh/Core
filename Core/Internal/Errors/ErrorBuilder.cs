using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{

    /// <summary>
    /// An error without a location.
    /// </summary>
    public class ErrorBuilder : IErrorBuilder
    {
        /// <summary>
        /// Create a new error.
        /// </summary>
        public ErrorBuilder(ErrorCode errorCode, params object?[] args)
        {
            Exception = null;
            ErrorCode = errorCode;
            Args      = args;
        }

        /// <summary>
        /// Create a new error from an exception.
        /// </summary>
        public ErrorBuilder(Exception exception,  ErrorCode errorCode)
        {
            Exception = exception;
            ErrorCode = errorCode;
            Args      = Array.Empty<object?>();
        }


        /// <summary>
        /// Associated Exception if there is one
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// Error Arguments.
        /// </summary>
        public object?[] Args { get; }

        /// <summary>
        /// Returns a SingleError with the given location.
        /// </summary>
        public SingleError WithLocationSingle(IErrorLocation location)
        {
            if (Exception != null)
                return new SingleError(location, Exception, ErrorCode);

            return new SingleError(location, ErrorCode, Args);
        }

        /// <inheritdoc />
        public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

        /// <inheritdoc />
        public IEnumerable<ErrorBuilder> GetErrorBuilders()
        {
            yield return this;
        }

        /// <inheritdoc />
        public string AsString => ErrorCode.GetFormattedMessage(Args);

        /// <inheritdoc />
        public override string ToString() => AsString;
    }
}