using System;
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// An single error.
    /// </summary>
    public class SingleError : IError
    {
        /// <summary>
        /// Create a new error.
        /// </summary>
        public SingleError(string message, string stepName, IError? innerError, ErrorCode errorCode)
        {
            Message = message;
            StepName = stepName;
            InnerError = innerError;
            Exception = null;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Create a new error with an exception.
        /// </summary>
        public SingleError(Exception exception, string stepName, ErrorCode errorCode)
        {
            Message = exception.Message;
            StepName = stepName;
            InnerError = null;
            Exception = exception;
            ErrorCode = errorCode;
        }


        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The name of the step that threw this error.
        /// </summary>
        public string StepName { get; }

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

        /// <inheritdoc />
        public IEnumerable<SingleError> GetAllErrors()
        {
            yield return this;
        }

        /// <inheritdoc />
        public string AsString => Message;
    }
}