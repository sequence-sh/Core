using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{
    /// <summary>
    /// Contains methods for interacting with logging.
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// Logs a message for the particular situation.
        /// Will use the resource to
        /// </summary>
        public static void LogSituation(this ILogger logger, LogSituationCore situationCore, IEnumerable<object> args)
            => LogSituation(logger, situationCore, LogSituationHelperCoreEN.Instance, args);

        /// <summary>
        /// Logs a message for the particular situation.
        /// Will use the resource to
        /// </summary>
        public static void LogSituation<T>(this ILogger logger, T situationCore, ILogSituationHelper<T> helper, IEnumerable<object> args) where T: Enum
        {
            var logLevel = helper.GetLogLevel(situationCore);
            if (logger.IsEnabled(logLevel))
            {
                var messageString = helper.GetMessageString(situationCore);
                logger.Log(logLevel, messageString, args.ToArray());
            }
        }


    }


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

    /// <summary>
    /// Contains methods for handling Log Severity and Localization.
    /// Each connector should have an implementation of this.
    /// </summary>
    public interface ILogSituationHelper<in T> where T : Enum
    {
        /// <summary>
        /// Gets the severity of the situation.
        /// </summary>
        LogLevel GetLogLevel(T logSituation);

        /// <summary>
        /// Gets a localized log message
        /// </summary>
        string GetMessageString(T logSituation);
    }

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
