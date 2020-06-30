namespace Reductech.EDR.Processes.Output
{
    /// <summary>
    /// The partial output of a process.
    /// </summary>
    public interface IProcessOutput<out T> : IProcessOutput
    {
        /// <summary>
        /// The value of this process.
        /// Will throw an exception if this is not a success result.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// The partial output of a process.
    /// </summary>
    public interface IProcessOutput
    {
        /// <summary>
        /// Gets the output in text format.
        /// </summary>
        /// <returns></returns>
        public string Text { get; }

        /// <summary>
        /// The type of this output - indicates that you can cast this object to the relevant struct.
        /// </summary>
        public OutputType OutputType { get; }

        /// <summary>
        /// Converts this to an output of a different type. Does not work on success results.
        /// </summary>
        /// <typeparam name="TN"></typeparam>
        /// <returns></returns>
        public IProcessOutput<TN> ConvertTo<TN>();
    }
}