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
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class ExternalProcessTests
{
    [Fact]
    public async Task RunExternalProcess_MustRunProcess()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = new StateMonad(
            loggerFactory.CreateLogger("ExternalProcess"),
            SCLSettings.EmptySettings,
            StepFactoryStore.Create(),
            repo.OneOf<IExternalContext>(),
            new Dictionary<string, object>()
        );

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
    public async Task RunExternalProcess_MustReturnErrors()
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = new StateMonad(
            loggerFactory.CreateLogger("ExternalProcess"),
            SCLSettings.EmptySettings,
            StepFactoryStore.Create(),
            repo.OneOf<IExternalContext>(),
            new Dictionary<string, object>()
        );

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
        var repo = new MockRepository(MockBehavior.Strict);

        var loggerFactory =
            MELT.TestLoggerFactory.Create(x => x.FilterByMinimumLevel(LogLevel.Information));

        var stateMonad = new StateMonad(
            loggerFactory.CreateLogger("ExternalProcess"),
            SCLSettings.EmptySettings,
            StepFactoryStore.Create(),
            repo.OneOf<IExternalContext>(),
            new Dictionary<string, object>()
        );

        var result = await ExternalProcessRunner.Instance.RunExternalProcess(
            "pwsh",
            new SelectiveErrorHandler(".*test error.*"),
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
