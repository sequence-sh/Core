using Microsoft.Extensions.Logging;

namespace Reductech.Sequence.Core.Internal.Logging;

/// <summary>
/// A log message.
/// These will be passed to the ILogger instance.
/// </summary>
public record LogMessage(
    string Message,
    object? MessageParams,
    string? StepName,
    TextLocation? Location,
    LogSituationBase? LogSituation,
    DateTime DateTime,
    IReadOnlyDictionary<string, object> SequenceInfo) : IEnumerable<KeyValuePair<string, object>
>
{
    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(nameof(Message), Message);

        if (MessageParams is not null)
            yield return new KeyValuePair<string, object>(nameof(MessageParams), MessageParams);

        if (StepName is not null)
            yield return new KeyValuePair<string, object>(nameof(StepName), StepName);

        if (Location is not null)
            yield return new KeyValuePair<string, object>(nameof(Location), Location);

        if (LogSituation is not null)
            yield return new KeyValuePair<string, object>(nameof(LogSituation), LogSituation);

        yield return new KeyValuePair<string, object>(nameof(DateTime),     DateTime);
        yield return new KeyValuePair<string, object>(nameof(SequenceInfo), SequenceInfo);
    }

    /// <inheritdoc />
    public override string ToString() => Message;

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
        params object?[] args) where T : LogSituationBase => LogSituation(
        logger,
        situation,
        step,
        monad.SequenceMetadata,
        args
    );

    /// <summary>
    /// Logs a message for the particular situation.
    /// Will use the resource to localize the message
    /// </summary>
    public static void LogSituation<T>(
        this ILogger logger,
        T situation,
        IStep? step,
        IReadOnlyDictionary<string, object> sequenceMetadata,
        object?[] args) where T : LogSituationBase
    {
        var logLevel = situation.LogLevel;

        if (!logger.IsEnabled(logLevel))
            return;

        var (message, properties) = situation.GetLocalizedString(args);

        var logMessage = new LogMessage(
            message,
            properties,
            step?.Name,
            step?.TextLocation,
            situation,
            DateTime.Now,
            sequenceMetadata
        );

        logger.Log(logLevel, default, logMessage, null, (x, _) => x.ToString());
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
            null,
            DateTime.Now,
            monad.SequenceMetadata
        );

        logger.Log(level, default, logMessage, null, (x, _) => x.Message);
    }

    /// <summary>
    /// Logs a Core error
    /// </summary>
    public static void LogError(this ILogger logger, IError error)
    {
        const string errorFormat = "{Error} (Step: {StepName}{Location})";

        foreach (var singleError in error.GetAllErrors())
        {
            var loc = singleError.Location.TextLocation == null
                ? string.Empty
                : $" {singleError.Location.TextLocation}";

            var stepName = singleError.Location.StepName ?? "n/a";

            if (singleError.Exception != null)
                logger.LogError(
                    singleError.Exception,
                    errorFormat,
                    singleError.Message,
                    stepName,
                    loc
                );
            else
                logger.LogError(errorFormat, singleError.Message, stepName, loc);
        }
    }
}
