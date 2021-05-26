using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class ExternalProcessTests
{
    public static StateMonad CreateStateMonad(ILogger logger)
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var stateMonad = new StateMonad(
            logger,
            SCLSettings.EmptySettings,
            StepFactoryStore.Create(),
            repo.OneOf<IExternalContext>(),
            new Dictionary<string, object>()
        );

        return stateMonad;
    }

    [Fact]
    public async Task RunExternalProcess_ShouldErrorIfProcessIsMissing()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "MyRidiculousImpossibleProcess",
            IgnoreNoneErrorHandler.Instance,
            new List<string>() { "--command", "write-output 'hello'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeFailure(
            ErrorCode.ExternalProcessNotFound.ToErrorBuilder("MyRidiculousImpossibleProcess")
        );
    }

    [Fact]
    public async Task RunExternalProcess_MustRunProcess()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            IgnoreNoneErrorHandler.Instance,
            new List<string>() { "--command", "write-output 'hello'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeSuccessful();

        var messages = loggerFactory.Sink.LogEntries.Select(x => x.Message).ToList();

        messages.Should().BeEquivalentTo("hello");
    }

    [Fact]
    public async Task RunExternalProcess_MustPassEnvironmentVariables()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            IgnoreNoneErrorHandler.Instance,
            new List<string>() { "--command", "write-output $env:TestVariable" },
            new Dictionary<string, string>() { { "TestVariable", "hello" } },
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeSuccessful();

        var messages = loggerFactory.Sink.LogEntries.Select(x => x.Message).ToList();

        messages.Should().BeEquivalentTo("hello");
    }

    [Fact]
    public async Task RunExternalProcess_MustReturnErrors()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            IgnoreNoneErrorHandler.Instance,
            new List<string>() { "--command", "write-error 'test error'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeFailure();
        result.Error.AsString.Should().Contain("test error");
    }

    [Fact]
    public async Task RunExternalProcess_ShouldIgnoreErrorsCorrectly()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            IgnoreAllErrorHandler.Instance,
            new List<string>() { "--command", "write-error 'test error'; write-output 'hello'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeSuccessful();

        var messages = loggerFactory.Sink.LogEntries.Select(x => x.Message).ToList();

        messages.Should().BeEquivalentTo("[91mWrite-Error: [91mtest error[0m", "hello");
    }

    [Fact]
    public async Task RunExternalProcess_ShouldIgnoreErrorsCorrectly2()
    {
        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = CreateStateMonad(loggerFactory.CreateLogger("ExternalProcess"));

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            new SelectiveErrorHandler(".*ignore this error.*"),
            new List<string>()
            {
                "--command", "write-error 'ignore this error'; write-error 'test error'"
            },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null,
            new CancellationToken()
        );

        result.ShouldBeFailure();
        result.Error.AsString.Should().Contain("test error");
    }

    private class SelectiveErrorHandler : IErrorHandler
    {
        public readonly string IgnoreRegex;

        public SelectiveErrorHandler(string regex)
        {
            IgnoreRegex = regex;
        }

        /// <inheritdoc />
        public bool ShouldIgnoreError(string s)
        {
            var r = Regex.IsMatch(s, IgnoreRegex);
            return r;
        }
    }
}

}
