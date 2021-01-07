namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Different LoggingSituations
/// </summary>
public enum LogSituationCore
{
    /// <summary>
    /// Whenever a step is entered.
    /// </summary>
    EnterStep,
    /// <summary>
    /// Whenever a step is exited after success.
    /// </summary>
    ExitStepSuccess,
    /// <summary>
    /// Whenever a step is existed after failure.
    /// </summary>
    ExitStepFailure,

    /// <summary>
    /// When a path is not fully qualified.
    /// </summary>
    QualifyingPath,
    /// <summary>
    /// When Path.Combine is given an empty list of paths.
    /// </summary>
    NoPathProvided,
    /// <summary>
    /// Directory Deleted
    /// </summary>
    DirectoryDeleted,
    /// <summary>
    /// File Deleted
    /// </summary>
    FileDeleted,
    /// <summary>
    /// Item to delete did not exist
    /// </summary>
    ItemToDeleteDidNotExist
}

}