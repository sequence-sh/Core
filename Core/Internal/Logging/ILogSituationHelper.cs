using System;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Logging
{

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

}