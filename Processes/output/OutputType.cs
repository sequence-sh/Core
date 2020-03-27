namespace Reductech.EDR.Utilities.Processes.output
{
    /// <summary>
    /// The type of this result
    /// </summary>
    public enum OutputType
    {
        /// <summary>
        /// An error message.
        /// </summary>
        Error,
        /// <summary>
        /// A warning message.
        /// </summary>
        Warning,
        /// <summary>
        /// A progress or log message.
        /// </summary>
        Message,
        /// <summary>
        /// A success value.
        /// </summary>
        Success
    }
}