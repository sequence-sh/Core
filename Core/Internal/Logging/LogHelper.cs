using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// A log message.
/// These will be passed to the ILogger instance.
/// </summary>
public record LogMessage(
        #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        string Message,
        object? MessageParams,
        string? StepName,
        TextLocation? Location,
        object SequenceInfo)
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public override string ToString() => Message;
}

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
        IStep? step,
        IStateMonad monad,
        params object?[] args) where T : LogSituationBase
    {
        var logLevel = situation.LogLevel;

        if (logger.IsEnabled(logLevel))
        {
            var q = situation.GetLocalizedString(args);

            var logMessage = new LogMessage(
                q.message,
                q.properties,
                step?.Name,
                step?.TextLocation,
                monad.SequenceMetadata
            );

            logger.Log(logLevel, default, logMessage, null, (x, _) => x.ToString());
        }
    }

    /// <summary>
    /// Logs a message that is not associated with a particular situation.
    /// Usually from an external process
    /// </summary>
    public static void LogMessage(
        this ILogger logger,
        LogLevel level,
        string message,
        IStep? step,
        IStateMonad monad)
    {
        var logMessage = new LogMessage(
            message,
            null,
            step?.Name,
            step?.TextLocation,
            monad.SequenceMetadata
        );

        logger.Log(level, default, logMessage, null, (x, _) => x.Message);
    }
}

}
