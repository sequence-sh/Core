using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Reductech.EDR.Core.TestHarness
{
    /// <summary>
    /// Only enabled for logs at information or higher.
    /// </summary>
    public class TestInformationLogger : ILogger
    {
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is IEnumerable<KeyValuePair<string, object>> flv)
                foreach (var (_, value) in flv)
                    LoggedValues.Add(value);
            else throw new NotImplementedException();

        }

        public List<object> LoggedValues = new();

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => false,
                LogLevel.Debug => false,
                LogLevel.Information => true,
                LogLevel.Warning => true,
                LogLevel.Error => true,
                LogLevel.Critical => true,
                LogLevel.None => false,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) => new ScopeDisposable();

        private sealed class ScopeDisposable : IDisposable
        {
            /// <inheritdoc />
            public void Dispose() {}
        }
    }
}