using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

/// <summary>
/// Identifying code for the log situation.
/// </summary>
public abstract record LogSituation(string Code, LogLevel LogLevel)
{
    /// <summary>
    /// Get the format string for this log situation
    /// </summary>
    public abstract string GetLocalizedString();

    /// <summary>
    /// The Error Code.
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// The log visibility level.
    /// </summary>
    public LogLevel LogLevel { get; init; } = LogLevel;
}

}
