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
}