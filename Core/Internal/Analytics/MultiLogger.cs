using System;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.Internal.Analytics;

/// <summary>
/// An instance of a step starting
/// </summary>
public record StepStart(DateTime Time, string StepName, TextLocation Location);

/// <summary>
/// An instance of a step ending
/// </summary>
public record StepEnd(
    DateTime Time,
    string StepName,
    TextLocation Location,
    Maybe<string> StepError);

/// <summary>
/// Log messages are sent to multiple loggers`
/// </summary>
public class MultiLogger : ILogger
{
    private readonly ILogger[] _loggers;

    /// <summary>
    /// Create a new MultiLogger
    /// </summary>
    public MultiLogger(params ILogger[] loggers) { _loggers = loggers; }

    /// <inheritdoc />
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        foreach (var logger in _loggers.Where(x => x.IsEnabled(logLevel)))
            logger.Log(logLevel, eventId, state, exception, formatter);
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return _loggers.Any(x => x.IsEnabled(logLevel));
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
    {
        var ds = _loggers.Select(x => x.BeginScope(state)).ToArray();
        return new MultiDisposable(ds);
    }

    private class MultiDisposable : IDisposable
    {
        private readonly IDisposable[] _disposables;

        public MultiDisposable(IDisposable[] disposables) { _disposables = disposables; }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}
