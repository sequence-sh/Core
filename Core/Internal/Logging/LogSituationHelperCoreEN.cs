using System;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Contains methods for handling Log Severity and Localization for Core in English.
/// </summary>
public sealed class LogSituationHelper_Core_EN : ILogSituationHelper<LogSituation_Core>
{
    private LogSituationHelper_Core_EN() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static ILogSituationHelper<LogSituation_Core> Instance { get; } =
        new LogSituationHelper_Core_EN();

    /// <inheritdoc />
    public LogLevel GetLogLevel(LogSituation_Core logSituation)
    {
        return logSituation switch
        {
            LogSituation_Core.EnterStep => LogLevel.Trace,
            LogSituation_Core.ExitStepFailure => LogLevel.Warning,
            LogSituation_Core.ExitStepSuccess => LogLevel.Trace,
            LogSituation_Core.QualifyingPath => LogLevel.Debug,
            LogSituation_Core.NoPathProvided => LogLevel.Warning,
            LogSituation_Core.DirectoryDeleted => LogLevel.Debug,
            LogSituation_Core.FileDeleted => LogLevel.Debug,
            LogSituation_Core.ItemToDeleteDidNotExist => LogLevel.Debug,
            LogSituation_Core.SetVariableOutOfScope => LogLevel.Warning,
            LogSituation_Core.RemoveVariableOutOfScope => LogLevel.Warning,
            _ => throw new ArgumentOutOfRangeException(nameof(logSituation), logSituation, null)
        };
    }

    /// <inheritdoc />
    public string GetMessageString(LogSituation_Core logSituation)
    {
        return logSituation switch
        {
            LogSituation_Core.EnterStep => LogMessages_EN.EnterStep,
            LogSituation_Core.ExitStepSuccess => LogMessages_EN.ExitStepSuccess,
            LogSituation_Core.ExitStepFailure => LogMessages_EN.ExitStepFailure,
            LogSituation_Core.QualifyingPath => LogMessages_EN.QualifyingPath,
            LogSituation_Core.NoPathProvided => LogMessages_EN.NoPathProvided,
            LogSituation_Core.DirectoryDeleted => LogMessages_EN.DirectoryDeleted,
            LogSituation_Core.FileDeleted => LogMessages_EN.FileDeleted,
            LogSituation_Core.ItemToDeleteDidNotExist => LogMessages_EN.ItemToDeleteDidNotExist,
            LogSituation_Core.SetVariableOutOfScope => LogMessages_EN.SetVariableOutOfScope,
            LogSituation_Core.RemoveVariableOutOfScope => LogMessages_EN.RemoveVariableOutOfScope,
            _ => throw new ArgumentOutOfRangeException(nameof(logSituation), logSituation, null)
        };
    }
}

}
