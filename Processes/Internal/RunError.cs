
using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// The base of run errors.
    /// </summary>
    public interface IRunErrorBase
    {
        /// <summary>
        /// The error as a string.
        /// </summary>
        string AsString { get; }
    }

    /// <summary>
    /// One or more errors thrown by a running process.
    /// </summary>
    public interface IRunErrors : IRunErrorBase
    {
        /// <summary>
        /// The errors.
        /// </summary>
        IEnumerable<IRunError> AllErrors { get; }
    }

    /// <summary>
    /// An error thrown by a running process.
    /// </summary>
    public interface IRunError : IRunErrorBase
    {
        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The name of the process that threw this error.
        /// </summary>
        public string ProcessName { get; }

        /// <summary>
        /// The error that caused this error.
        /// </summary>
        public RunError? InnerError { get; }

        /// <summary>
        /// The error code.
        /// </summary>
        public ErrorCode ErrorCode { get; }
    }

    /// <summary>
    /// A list of errors thrown by a running process.
    /// </summary>
    public class RunErrorList : IRunErrors
    {
        /// <summary>
        /// Create a new RunErrorList
        /// </summary>
        /// <param name="allErrors"></param>
        public RunErrorList(IReadOnlyCollection<IRunError> allErrors) => AllErrors = allErrors;

        /// <inheritdoc />
        public IEnumerable<IRunError> AllErrors { get; }

        /// <inheritdoc />
        public string AsString =>
            string.Join("; ", AllErrors.Select(x => x.AsString));

        /// <summary>
        /// Combine multiple run errors.
        /// </summary>
        public static IRunErrors Combine(IEnumerable<IRunErrors> source) => new RunErrorList(source.SelectMany(x=>x.AllErrors).ToList());
    }

    /// <summary>
    /// An error thrown by a running process.
    /// </summary>
    public class RunError : IRunErrors, IRunError
    {
        /// <summary>
        /// Create a new RunError.
        /// </summary>
        public RunError(string message, string processName, RunError? innerError, ErrorCode errorCode)
        {
            Message = message;
            ProcessName = processName;
            InnerError = innerError;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Error Message Text.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The name of the process that threw this error.
        /// </summary>
        public string ProcessName { get; }

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

    /// <summary>
    /// Identifying code for an error message.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Variable does not exist.
        /// </summary>
        MissingVariable,

        /// <summary>
        /// Variable has the wrong type.
        /// </summary>
        WrongVariableType,

        /// <summary>
        /// Index was out of the range of an array or string.
        /// </summary>
        IndexOutOfBounds,

        /// <summary>
        /// An error in an external process.
        /// </summary>
        ExternalProcessError,

        /// <summary>
        /// The external process was not found.
        /// </summary>
        ExternalProcessNotFound,

        /// <summary>
        /// The requirements for a process were not met.
        /// </summary>
        RequirementsNotMet,

        /// <summary>
        /// Cast failed.
        /// </summary>
        InvalidCast,

        /// <summary>
        /// Process settings are missing
        /// </summary>
        MissingProcessSettings

    }
}
