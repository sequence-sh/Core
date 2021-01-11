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
    /// Will use the resource to localize the message
    /// </summary>
    public static void LogSituation<T>(
        this ILogger logger,
        T situation,
        IEnumerable<object?> args) where T : LogSituation
    {
        var logLevel = situation.LogLevel;

        if (logger.IsEnabled(logLevel))
        {
            var messageString = situation.GetLocalizedString();
            logger.Log(logLevel, messageString, args.ToArray());
        }
    }
}

}
