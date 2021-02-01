using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Identifying code for a Core log situation.
/// </summary>
public sealed record LogSituation : LogSituationBase
{
    private LogSituation(string code, LogLevel logLevel) : base(code, logLevel) { }

    /// <inheritdoc />
    public override string GetLocalizedString()
    {
        var localizedMessage = LogMessages_EN
            .ResourceManager.GetString(Code); //TODO static method to get this

        Debug.Assert(localizedMessage != null, nameof(localizedMessage) + " != null");
        return localizedMessage;
    }

    /// <summary>
    /// Directory Deleted: {Path}
    /// </summary>
    public static readonly LogSituation DirectoryDeleted = new(
        nameof(DirectoryDeleted),
        LogLevel.Debug
    );

    /// <summary>
    /// {StepName} Started with Parameters: {Parameters}
    /// </summary>
    public static readonly LogSituation EnterStep = new(nameof(EnterStep), LogLevel.Trace);

    /// <summary>
    /// {StepName} Failed with message: {Message}
    /// </summary>
    public static readonly LogSituation ExitStepFailure = new(
        nameof(ExitStepFailure),
        LogLevel.Warning
    );

    /// <summary>
    /// {StepName} Completed Successfully with Result: {Result}
    /// </summary>
    public static readonly LogSituation ExitStepSuccess = new(
        nameof(ExitStepSuccess),
        LogLevel.Trace
    );

    /// <summary>
    /// File Deleted: {Path}
    /// </summary>
    public static readonly LogSituation FileDeleted = new(nameof(FileDeleted), LogLevel.Debug);

    /// <summary>
    /// Item to Delete did not Exist: {Path}
    /// </summary>
    public static readonly LogSituation ItemToDeleteDidNotExist = new(
        nameof(ItemToDeleteDidNotExist),
        LogLevel.Debug
    );

    /// <summary>
    /// No path was provided. Returning the Current Directory: {CurrentDirectory}
    /// </summary>
    public static readonly LogSituation NoPathProvided = new(
        nameof(NoPathProvided),
        LogLevel.Warning
    );

    /// <summary>
    /// Path {Path} was not fully qualified. Prepending the Current Directory: {CurrentDirectory}
    /// </summary>
    public static readonly LogSituation QualifyingPath = new(
        nameof(QualifyingPath),
        LogLevel.Debug
    );

    /// <summary>
    /// Could not remove variable {variable} as it was out of scope.
    /// </summary>
    public static readonly LogSituation RemoveVariableOutOfScope = new(
        nameof(RemoveVariableOutOfScope),
        LogLevel.Warning
    );

    /// <summary>
    /// Could not set variable {variable} as it was out of scope.
    /// </summary>
    public static readonly LogSituation SetVariableOutOfScope = new(
        nameof(SetVariableOutOfScope),
        LogLevel.Warning
    );

    /// <summary>
    /// Schema violation: {message}
    /// </summary>
    public static readonly LogSituation SchemaViolation = new(
        nameof(SchemaViolation),
        LogLevel.Warning
    );
}

}
