namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// An error thrown by a running step.
    /// </summary>
    public interface IRunError : IRunErrorBase
    {
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
    }
}