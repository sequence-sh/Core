using System;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Logging
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
        public static void LogSituation(this ILogger logger, LogSituation situation, params object[] args)
        {
            using (logger.BeginScope(situation))
            {
                var logLevel = GetLogLevel(situation);
                var messageString = GetMessageString(situation);
                logger.Log(logLevel, messageString, args);
            }
        }

        private static string GetMessageString(LogSituation situation)
        {
            return situation switch
            {
                Logging.LogSituation.EnterStep => LogMessages_EN.EnterStep,
                Logging.LogSituation.ExitStepSuccess => LogMessages_EN.ExitStepSuccess,
                Logging.LogSituation.ExitStepFailure => LogMessages_EN.ExitStepFailure,
                _ => throw new ArgumentOutOfRangeException(nameof(situation), situation, null)
            };
        }

        private static LogLevel GetLogLevel(LogSituation situation)
        {
            return situation switch
            {
                Logging.LogSituation.EnterStep => LogLevel.Trace,
                Logging.LogSituation.ExitStepFailure => LogLevel.Trace,
                Logging.LogSituation.ExitStepSuccess => LogLevel.Trace,
                _ => throw new ArgumentOutOfRangeException(nameof(situation), situation, null)
            };
        }


    }

    /// <summary>
    /// Different LoggingSituations
    /// </summary>
    public enum LogSituation
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
    }
}
