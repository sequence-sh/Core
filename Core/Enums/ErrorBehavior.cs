namespace Reductech.EDR.Core.Enums
{

/// <summary>
/// How to respond to a data error
/// </summary>
public enum ErrorBehavior
{
    /// <summary>
    /// On Error: Stop the process
    /// </summary>
    Fail,

    /// <summary>
    ///  On Error: Emit a warning and skip the entity
    /// </summary>
    Error,

    /// <summary>
    /// On Error: Emit a warning but do allow the entity
    /// </summary>
    Warning,

    /// <summary>
    ///  On Error: Skip the entity but do not emit a warning
    /// </summary>
    Skip,

    /// <summary>
    /// On Error: Allow the entity and do not emit a warning
    /// </summary>
    Ignore
}

}
