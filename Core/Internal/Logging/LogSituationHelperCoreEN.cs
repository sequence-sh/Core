using System;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Contains methods for handling Log Severity and Localization for Core in English.
/// </summary>
public sealed class LogSituationHelperCoreEN : ILogSituationHelper<LogSituationCore>
{
    private LogSituationHelperCoreEN() { }

    /// <summary>
    /// The Instance
    /// </summary>
    public static ILogSituationHelper<LogSituationCore> Instance { get; } = new LogSituationHelperCoreEN();

    /// <inheritdoc />
    public LogLevel GetLogLevel(LogSituationCore logSituation)
    {
        return logSituation switch
        {
            LogSituationCore.EnterStep => LogLevel.Trace,
            LogSituationCore.ExitStepFailure => LogLevel.Warning,
            LogSituationCore.ExitStepSuccess => LogLevel.Trace,
            LogSituationCore.QualifyingPath => LogLevel.Debug,
            LogSituationCore.NoPathProvided => LogLevel.Warning,
            LogSituationCore.DirectoryDeleted => LogLevel.Debug,
            LogSituationCore.FileDeleted => LogLevel.Debug,
            LogSituationCore.ItemToDeleteDidNotExist => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException(nameof(logSituation), logSituation, null)
        };
    }

    /// <inheritdoc />
    public string GetMessageString(LogSituationCore logSituation)
    {
        return logSituation switch
        {
            LogSituationCore.EnterStep => LogMessages_EN.EnterStep,
            LogSituationCore.ExitStepSuccess => LogMessages_EN.ExitStepSuccess,
            LogSituationCore.ExitStepFailure => LogMessages_EN.ExitStepFailure,
            LogSituationCore.QualifyingPath => LogMessages_EN.QualifyingPath,
            LogSituationCore.NoPathProvided => LogMessages_EN.NoPathProvided,
            LogSituationCore.DirectoryDeleted => LogMessages_EN.DirectoryDeleted,
            LogSituationCore.FileDeleted => LogMessages_EN.FileDeleted,
            LogSituationCore.ItemToDeleteDidNotExist => LogMessages_EN.ItemToDeleteDidNotExist,
            _ => throw new ArgumentOutOfRangeException(nameof(logSituation), logSituation, null)
        };
    }
}

}