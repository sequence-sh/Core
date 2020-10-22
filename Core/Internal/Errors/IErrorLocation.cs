namespace Reductech.EDR.Core.Internal.Errors
{
    /// <summary>
    /// The location from which the error originates.
    /// </summary>
    public interface IErrorLocation
    {
        /// <summary>
        /// The error location as a string.
        /// </summary>
        string AsString { get; }
    }
}