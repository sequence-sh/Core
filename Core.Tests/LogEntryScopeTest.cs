using System;
using AutoTheory;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[UseTestOutputHelper]
public partial class LogEntryScopeTest
{
    [Fact(Skip = "Example test for a MELT issue")]
    public void LogEntryScopeShouldBeMostInnerOpenScope()
    {
        var loggerFactory = TestLoggerFactory.Create();
        loggerFactory.AddXunit(TestOutputHelper);

        var logger = loggerFactory.CreateLogger("Test");

        using (logger.BeginScope("Outer Scope"))
        {
            logger.LogInformation("Message 1");

            using (logger.BeginScope("Inner Scope"))
            {
                logger.LogInformation("Message 2");
            }

            logger.LogInformation("Message 3");
        }

        loggerFactory.Sink.LogEntries
            .Should()
            .SatisfyRespectively(
                CheckMessageAndScope("Message 1", "Outer Scope"),
                CheckMessageAndScope("Message 2", "Inner Scope"),
                CheckMessageAndScope("Message 3", "Outer Scope")
            );

        static Action<LogEntry> CheckMessageAndScope(string expectedMessage, string expectedScope)
        {
            return entry =>
            {
                entry.Message.Should().Be(expectedMessage);
                entry.Scope.Message.Should().Be(expectedScope);
            };
        }
    }
}

}
