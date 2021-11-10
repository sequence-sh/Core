using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Internal.Logging;

namespace Reductech.EDR.Core.Internal.Analytics
{

/// <summary>
/// A logger that logs analytics messages
/// </summary>
public class AnalyticsLogger : ILogger
{
    /// <summary>
    /// The Analytics for the sequence
    /// </summary>
    public SequenceAnalytics SequenceAnalytics { get; } = new();

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (state is LogMessage { StepName: { }, Location: { } } logMessage)
        {
            if (logMessage.LogSituation == LogSituation.EnterStep)
            {
                SequenceAnalytics.StepStarts.Add(
                    new StepStart(
                        logMessage.DateTime,
                        logMessage.StepName,
                        logMessage.Location
                    )
                );
            }
            else if (logMessage.LogSituation == LogSituation.ExitStepSuccess)
            {
                SequenceAnalytics.StepEnds.Add(
                    new StepEnd(
                        logMessage.DateTime,
                        logMessage.StepName,
                        logMessage.Location,
                        Maybe<string>.None
                    )
                );
            }
            else if (logMessage.LogSituation == LogSituation.ExitStepFailure)
            {
                string errorMessage;

                if (logMessage.MessageParams is Dictionary<string, object> dict
                 && dict.TryGetValue("Message", out var m))
                    errorMessage = m?.ToString()!;
                else
                    errorMessage = logMessage.Message;

                SequenceAnalytics.StepEnds.Add(
                    new StepEnd(
                        logMessage.DateTime,
                        logMessage.StepName,
                        logMessage.Location,
                        Maybe<string>.From(errorMessage)
                    )
                );
            }
        }
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        return new EmptyDisposable();
    }

    private class EmptyDisposable : IDisposable
    {
        /// <inheritdoc />
        public void Dispose() { }
    }
}

}
