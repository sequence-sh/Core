﻿using System;
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
        /// Create a new error.
        /// </summary>
        public SingleError(string message, ErrorCode errorCode, IErrorLocation location, IError? innerError = null)
        {
            Message = message;
            Location = location;
            InnerError = innerError;
            Exception = null;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Create a new error with an exception.
        /// </summary>
        public SingleError(Exception exception, ErrorCode errorCode, IErrorLocation location)
        {
            Message = exception.Message;
            Location = location;
            InnerError = null;
            Exception = exception;
            ErrorCode = errorCode;
            Timestamp = DateTime.Now;
        }


        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The time the error was created.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The location where this error arose. This could be a line number.
        /// </summary>
        public IErrorLocation Location { get; }

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

        /// <inheritdoc />
        public override string ToString() => ErrorCode + ": " + AsString + " in " + Location.AsString;

        /// <inheritdoc />
        public bool Equals(IError? other)
        {
            return other switch
            {
                ErrorList errorList => errorList.GetAllErrors().Count() == 1 &&
                                       Equals(errorList.GetAllErrors().Single()),
                SingleError singleError => Equals(singleError),
                _ => false
            };
        }

        private bool Equals(SingleError other)
        {
            return Message == other.Message &&
                   Location.Equals(other.Location) &&
                   ErrorCode == other.ErrorCode;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is IError error && Equals(error);
        }

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Message, Location, (int) ErrorCode);
    }
}