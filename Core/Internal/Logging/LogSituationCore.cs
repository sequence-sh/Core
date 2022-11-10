using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Sequence.Core.Internal.Logging;

/// <summary>
/// Identifying code for a Core log situation.
/// </summary>
public sealed record LogSituation : LogSituationBase
{
    private LogSituation(string code, LogLevel logLevel) : base(code, logLevel) { }

    /// <inheritdoc />
    protected override string GetLocalizedString()
    {
        var localizedMessage = LogMessages_EN
            .ResourceManager.GetString(Code);

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
    /// Error Caught in {StepName}: {Message}
    /// </summary>
    public static readonly LogSituation StepErrorWasCaught = new(
        nameof(StepErrorWasCaught),
        LogLevel.Information
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
        LogLevel.Trace
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
    /// SchemaViolation: '{0}' at '{1}' on row '{2}', entity: '{3}'.
    /// </summary>
    public static readonly LogSituation SchemaViolated = new(
        nameof(SchemaViolated),
        LogLevel.Warning
    );

    /// <summary>
    /// ExternalProcess {process} started with arguments: '{arguments}'
    /// </summary>
    public static readonly LogSituation ExternalProcessStarted = new(
        nameof(ExternalProcessStarted),
        LogLevel.Debug
    );

    /// <summary>
    /// Sequence® Sequence Started
    /// </summary>
    public static readonly LogSituation SequenceStarted = new(
        nameof(SequenceStarted),
        LogLevel.Debug
    );

    /// <summary>
    /// ConnectorSettings: {settings}
    /// </summary>
    public static readonly LogSituation ConnectorSettings = new(
        nameof(ConnectorSettings),
        LogLevel.Trace
    );

    /// <summary>
    /// Sequence® Sequence Completed
    /// </summary>
    public static readonly LogSituation SequenceCompleted = new(
        nameof(SequenceCompleted),
        LogLevel.Debug
    );

    /// <summary>
    /// {EnvironmentVariableName}: {EnvironmentVariableValue}
    /// </summary>
    public static readonly LogSituation EnvironmentVariable = new(
        nameof(EnvironmentVariable),
        LogLevel.Trace
    );

    /// <summary>
    /// The id '{Id}' does not exist.
    /// </summary>
    public static readonly LogSituation IdNotPresent = new(
        nameof(IdNotPresent),
        LogLevel.Warning
    );
}
