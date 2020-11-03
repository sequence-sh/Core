using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// An exception that wraps an error
    /// </summary>
    public class ErrorBuilderException : Exception
    {
        /// <summary>
        /// Create a new ErrorException
        /// </summary>
        /// <param name="errorBuilder"></param>
        public ErrorBuilderException(IErrorBuilder errorBuilder) => ErrorBuilder = errorBuilder;

        /// <summary>
        /// The errorBuilder
        /// </summary>
        public IErrorBuilder ErrorBuilder { get; }
    }

    /// <summary>
    /// An exception that wraps an error
    /// </summary>
    public class ErrorException : Exception
    {
        /// <summary>
        /// Create a new ErrorException
        /// </summary>
        public ErrorException(IError error) => Error = error;

        /// <summary>
        /// The errorBuilder
        /// </summary>
        public IError Error { get; }
    }

    /// <summary>
    /// An error without a location.
    /// </summary>
    public class ErrorBuilder : IErrorBuilder
    {
        /// <summary>
        /// Create a new error.
        /// </summary>
        public ErrorBuilder(string message, ErrorCode errorCode, IError? innerError = null)
        {
            Message = message;
            InnerError = innerError;
            Exception = null;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Create a new error with an exception.
        /// </summary>
        public ErrorBuilder(Exception exception,  ErrorCode errorCode)
        {
            Message = exception.Message;
            InnerError = null;
            Exception = exception;
            ErrorCode = errorCode;
        }


        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The error that caused this error.
        /// </summary>
        public IError? InnerError { get; }

        /// <summary>
        /// Associated Exception if there is one
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }


        /// <summary>
        /// Returns a SingleError with the given location.
        /// </summary>
        public SingleError WithLocationSingle(IErrorLocation location)
        {
            if (Exception != null)
                return new SingleError(Exception, ErrorCode, location);

            return new SingleError(Message, ErrorCode, location, InnerError);
        }

        /// <inheritdoc />
        public IError WithLocation(IErrorLocation location) => WithLocationSingle(location);

        /// <inheritdoc />
        public IEnumerable<ErrorBuilder> GetErrorBuilders()
        {
            yield return this;
        }

        /// <inheritdoc />
        public string AsString => Message;

        /// <inheritdoc />
        public override string ToString() => AsString;
    }
}