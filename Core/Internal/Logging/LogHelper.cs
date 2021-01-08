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
    public static void LogSituation(
        this ILogger logger,
        LogSituation_Core situationCore,
        IEnumerable<object> args) => LogSituation(
        logger,
        situationCore,
        LogSituationHelper_Core_EN.Instance,
        args
    );

    /// <summary>
    /// Logs a message for the particular situation.
    /// Will use the resource to
    /// </summary>
    public static void LogSituation<T>(
        this ILogger logger,
        T situationCore,
        ILogSituationHelper<T> helper,
        IEnumerable<object> args) where T : Enum
    {
        var logLevel = helper.GetLogLevel(situationCore);

        if (logger.IsEnabled(logLevel))
        {
            var messageString = helper.GetMessageString(situationCore);
            logger.Log(logLevel, messageString, args.ToArray());
        }
    }
}

}
