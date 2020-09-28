namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// The case to convert the text to.
    /// </summary>
    public enum TextCase
    {
        /// <summary>
        /// All characters will be in upper case.
        /// </summary>
        Upper,
        /// <summary>
        /// All characters will be in lower case.
        /// </summary>
        Lower,
        /// <summary>
        /// Only the first character in each word will be in upper case.
        /// </summary>
        Title
    }
}