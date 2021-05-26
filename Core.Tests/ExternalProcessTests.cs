using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
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
            "pwsh.exe",
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
}

}
