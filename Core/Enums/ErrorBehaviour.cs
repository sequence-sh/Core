namespace Reductech.EDR.Core.Enums
{

/// <summary>
/// How to respond to a data error
/// </summary>
public enum ErrorBehaviour
{
    /// <summary>
    /// Stop the process on error
    /// </summary>
    Fail,

    /// <summary>
    /// Log a warning message on error
    /// </summary>
    Warning,

    /// <summary>
    /// Ignore errors
    /// </summary>
    Ignore
}

}
