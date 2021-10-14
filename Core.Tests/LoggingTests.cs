using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using AutoTheory;

namespace Reductech.EDR.Core.Tests
{

public partial class LoggingTests
{
    [GenerateAsyncTheory("CheckLogging")]
    #pragma warning disable CA1822 // Mark members as static
    public IEnumerable<LoggingTestCase> TestCases
        #pragma warning restore CA1822 // Mark members as static
    {
        get
        {
            yield return new LoggingTestCase(
                "Log 1",
                "Log 1",
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, 1]",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(LogLevel.Information, "1", new[] { "Log" }),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Completed Successfully with Result: Unit",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
            );

            yield return new LoggingTestCase(
                "Log 1 + 1",
                "Log (1 + 1)",
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, Sum]",
                    new[] { "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Sum Started with Parameters: [Terms, ArrayNew]",
                    new[] { "Log", "Sum", }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 2 Elements]",
                    new[] { "Log", "Sum", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 2 Elements",
                    new[] { "Log", "Sum", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Sum Completed Successfully with Result: 2",
                    new[] { "Log", "Sum" }
                ),
                CheckMessageAndScope(LogLevel.Information, "2", new[] { "Log", }),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Completed Successfully with Result: Unit",
                    new[] { "Log", }
                ),
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
            );

            yield return new LoggingTestCase(
                "Error",
                "AssertError (Log (1 / 0))",
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Started", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "AssertError Started with Parameters: [Step, Log]",
                    new[] { "AssertError" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Started with Parameters: [Value, Divide]",
                    new[] { "AssertError", "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Divide Started with Parameters: [Terms, ArrayNew]",
                    new[] { "AssertError", "Log", "Divide" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 2 Elements]",
                    new[] { "AssertError", "Log", "Divide", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 2 Elements",
                    new[] { "AssertError", "Log", "Divide", "ArrayNew" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Divide Failed with message: Attempt to Divide by Zero.",
                    new[] { "AssertError", "Log", "Divide" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Log Failed with message: Attempt to Divide by Zero.",
                    new[] { "AssertError", "Log" }
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "AssertError Completed Successfully with Result: Unit",
                    new[] { "AssertError" }
                ),
                CheckMessageAndScope(LogLevel.Debug, "EDR Sequence Completed", null)
            );
        }
    }

    private static Action<LogEntry> CheckMessageAndScope(
        LogLevel logLevel,
        string expectedMessage,
        IReadOnlyList<string>? expectedScopes)
    {
        return entry =>
        {
            entry.LogLevel.Should().Be(logLevel);
            entry.Message.Should().Be(expectedMessage);

            var trueExpectedScopes =
                expectedScopes is null
                    ? new List<string>() { "EDR" }
                    : expectedScopes.Prepend("EDR").ToList();

            entry.Scopes.Select(x => x.Message)
                .Should()
                .BeEquivalentTo(trueExpectedScopes);
        };
    }

    public record LoggingTestCase : IAsyncTestInstance, ICaseWithSetup
    {
        public LoggingTestCase(string name, string scl, params Action<LogEntry>[] expectedLogs)
        {
            Name         = name;
            SCL          = scl;
            ExpectedLogs = expectedLogs;
        }

        public string Name { get; set; }

        public string SCL { get; set; }
        public IReadOnlyList<Action<LogEntry>> ExpectedLogs { get; set; }

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var spf = StepFactoryStore.Create();

            var loggerFactory = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);

            var logger = loggerFactory.CreateLogger("Test");
            var repo   = new MockRepository(MockBehavior.Strict);

            var context           = ExternalContextSetupHelper.GetExternalContext(repo);
            var restClient        = RESTClientSetupHelper.GetRESTClient(repo, FinalChecks);
            var restClientFactory = new SingleRestClientFactory(restClient);

            var sclRunner = new SCLRunner(
                logger,
                spf,
                context,
                restClientFactory
            );

            var r = await sclRunner.RunSequenceFromTextAsync(
                SCL,
                new Dictionary<string, object>(),
                CancellationToken.None
            );

            r.ShouldBeSuccessful();

            foreach (var finalCheck in FinalChecks)
            {
                finalCheck();
            }

            loggerFactory.Sink.LogEntries.Should().SatisfyRespectively(ExpectedLogs);
        }

        /// <inheritdoc />
        public ExternalContextSetupHelper ExternalContextSetupHelper { get; } = new();

        /// <inheritdoc />
        public RESTClientSetupHelper RESTClientSetupHelper { get; } = new();

        /// <inheritdoc />
        public List<Action> FinalChecks { get; } = new();
    }
}

}
