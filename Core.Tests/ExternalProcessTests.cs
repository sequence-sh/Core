using System.Text;
using System.Text.RegularExpressions;
using Divergic.Logging.Xunit;
using Sequence.Core.ExternalProcesses;

namespace Sequence.Core.Tests;

[AutoTheory.UseTestOutputHelper]
public partial class ExternalProcessTests
{
#region RunExternalProcess

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

        messages.Should().HaveCount(2);

        messages.Should().Contain(x => x.Contains("Write-Error"));
        messages.Should().Contain(x => x.Contains("hello"));
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

#endregion

#region StartExternalProcess

    [Fact]
    public void StartExternalProcess_ShouldErrorIfProcessIsMissing()
    {
        var stateMonad =
            CreateStateMonad(new TestOutputLogger("ExternalProcess", TestOutputHelper));

        var startProcessResult = ExternalProcessRunner.Instance.StartExternalProcess(
            "MyRidiculousImpossibleProcess",
            new List<string>() { "--command", "write-output 'hello'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null
        );

        startProcessResult.ShouldBeFailure(
            ErrorCode.ExternalProcessNotFound.ToErrorBuilder("MyRidiculousImpossibleProcess")
        );
    }

    [Fact]
    public async Task StartExternalProcess_MustRunProcess()
    {
        var stateMonad =
            CreateStateMonad(new TestOutputLogger("ExternalProcess", TestOutputHelper));

        var startProcessResult = ExternalProcessRunner.Instance.StartExternalProcess(
            "pwsh",
            new List<string>() { "--command", "write-output 'hello'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null
        );

        startProcessResult.ShouldBeSuccessful();

        var outputLines = await startProcessResult.Value.OutputChannel
            .ReadAllAsync(CancellationToken.None)
            .ToListAsync();

        outputLines.Select(x => x.source).Should().AllBeEquivalentTo(StreamSource.Output);

        startProcessResult.Value.WaitForExit(null);

        outputLines.Select(x => x.line).Should().BeEquivalentTo("hello");
    }

    [Fact]
    public async Task StartExternalProcess_MustPassEnvironmentVariables()
    {
        var stateMonad =
            CreateStateMonad(new TestOutputLogger("ExternalProcess", TestOutputHelper));

        var startProcessResult = ExternalProcessRunner.Instance.StartExternalProcess(
            "pwsh",
            new List<string>() { "--command", "write-output $env:TestVariable" },
            new Dictionary<string, string>() { { "TestVariable", "hello" } },
            Encoding.Default,
            stateMonad,
            null
        );

        startProcessResult.ShouldBeSuccessful();

        var outputLines = await startProcessResult.Value.OutputChannel
            .ReadAllAsync(CancellationToken.None)
            .ToListAsync();

        outputLines.Select(x => x.source).Should().AllBeEquivalentTo(StreamSource.Output);

        startProcessResult.Value.WaitForExit(1000);
        startProcessResult.Value.Dispose();

        outputLines.Select(x => x.line).Should().BeEquivalentTo("hello");
    }

    [Fact]
    public async Task StartExternalProcess_MustOutputErrors()
    {
        var stateMonad =
            CreateStateMonad(new TestOutputLogger("ExternalProcess", TestOutputHelper));

        var startProcessResult = ExternalProcessRunner.Instance.StartExternalProcess(
            "pwsh",
            new List<string>() { "--command", "write-error 'test error'" },
            new Dictionary<string, string>(),
            Encoding.Default,
            stateMonad,
            null
        );

        startProcessResult.ShouldBeSuccessful();

        var outputLines = await startProcessResult.Value.OutputChannel
            .ReadAllAsync(CancellationToken.None)
            .ToListAsync();

        startProcessResult.Value.WaitForExit(1000);
        startProcessResult.Value.Dispose();

        outputLines.Should()
            .Contain(x => x.line.Contains("Write-Error") && x.source == StreamSource.Error);
    }

#endregion

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

    public static StateMonad CreateStateMonad(ILogger logger)
    {
        var repo = new MockRepository(MockBehavior.Strict);

        var stateMonad = new StateMonad(
            logger,
            StepFactoryStore.Create(),
            repo.OneOf<IExternalContext>(),
            new Dictionary<string, object>()
        );

        return stateMonad;
    }
}
