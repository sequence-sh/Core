using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

namespace Reductech.EDR.Processes.Test
{
    public class TestLogger : ILogger
    {
        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state is FormattedLogValues flv)
                foreach (var formattedLogValue in flv)
                    LoggedValues.Add(formattedLogValue.Value);
            else throw new NotImplementedException();
        }

        public List<object> LoggedValues = new List<object>();

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}