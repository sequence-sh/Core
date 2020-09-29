
using System.Collections.Generic;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// An error thrown by a running step.
    /// </summary>
    public class RunError : IRunErrors, IRunError
    {
        /// <summary>
        /// Create a new RunError.
        /// </summary>
        public RunError(string message, string stepName, RunError? innerError, ErrorCode errorCode)
        {
            Message = message;
            StepName = stepName;
            InnerError = innerError;
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
        public RunError? InnerError { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <inheritdoc />
        public IEnumerable<IRunError> AllErrors => new[] {this};

        /// <inheritdoc />
        public string AsString => Message;
    }
}
