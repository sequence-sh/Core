using Microsoft.Extensions.Logging;

namespace Sequence.Core.Enums;

/// <summary>
/// Log severity
/// </summary>
public enum Severity
{
    /// <summary>
    /// Verbose log messages that can be used for debugging SCL.
    /// </summary>
    Debug,

    /// <summary>
    /// Log messages that track the general flow of a sequence.
    /// </summary>
    Information,

    /// <summary>
    /// Log messages for non-terminating errors or unexpected events.
    /// </summary>
    Warning,

    /// <summary>
    /// Log messages that indicate sequence failure.
    /// </summary>
    Error
}

/// <summary>
/// Contains methods for converting SCL log severity to other log provider severity.
/// </summary>
public static class SeverityHelper
{
    /// <summary>
    /// Convert this to a Microsoft.Extensions.Logging.LogLevel.
    /// </summary>
    public static LogLevel ConvertToLogLevel(this Severity severity) => severity switch
    {
        Severity.Debug => LogLevel.Debug,
        Severity.Information => LogLevel.Information,
        Severity.Warning => LogLevel.Warning,
        Severity.Error => LogLevel.Error,
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };
}
