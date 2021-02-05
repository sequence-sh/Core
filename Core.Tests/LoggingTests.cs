using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using AutoTheory;
using Thinktecture;

namespace Reductech.EDR.Core.Tests
{

public partial class LoggingTests
{
    [GenerateAsyncTheory("CheckLogging")]
    public IEnumerable<LoggingTestCase> TestCases
    {
        get
        {
            yield return new LoggingTestCase(
                "Print 1",
                "Print 1",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, 1]",
                    "Print"
                ),
                CheckMessageAndScope(LogLevel.Information, "1", "Print"),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Completed Successfully with Result: Unit",
                    "Print"
                )
            );

            yield return new LoggingTestCase(
                "Print 1 + 1",
                "Print (1 + 1)",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, Sum]",
                    "Print"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Sum Started with Parameters: [Terms, ArrayNew]",
                    "Sum"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 2 Elements]",
                    "ArrayNew"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 2 Elements",
                    "ArrayNew"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Sum Completed Successfully with Result: 2",
                    "ArrayNew"
                ),
                CheckMessageAndScope(LogLevel.Information, "2", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Completed Successfully with Result: Unit",
                    null
                )
            );

            yield return new LoggingTestCase(
                "Error",
                "AssertError (Print (1 / 0))",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "AssertError Started with Parameters: [Step, Print]",
                    "AssertError"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, Divide]",
                    "Print"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Divide Started with Parameters: [Terms, ArrayNew]",
                    "Divide"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 2 Elements]",
                    "ArrayNew"
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 2 Elements",
                    "ArrayNew"
                ),
                CheckMessageAndScope(
                    LogLevel.Warning,
                    "Divide Failed with message: Attempt to Divide by Zero.",
                    "ArrayNew"
                ),
                CheckMessageAndScope(
                    LogLevel.Warning,
                    "Print Failed with message: Attempt to Divide by Zero.",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "AssertError Completed Successfully with Result: Unit",
                    null
                )
            );

            yield return new LoggingTestCase(
                "No Path to combine",
                "Print (PathCombine [])",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, PathCombine]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Started with Parameters: [Paths, ArrayNew]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 0 Elements]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 0 Elements",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Warning,
                    "No path was provided. Returning the Current Directory: MyDir",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Completed Successfully with Result: string Length: 5",
                    null
                ),
                CheckMessageAndScope(LogLevel.Information, @"MyDir", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Completed Successfully with Result: Unit",
                    null
                )
            )
            {
                FileSystemActions = new List<Action<Mock<IFileSystemHelper>>>
                {
                    x => x.Setup(f => f.GetCurrentDirectory()).Returns("MyDir")
                }
            };

            yield return new LoggingTestCase(
                "Unqualified Path to combine",
                "Print (PathCombine ['File'])",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, PathCombine]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Started with Parameters: [Paths, ArrayNew]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Started with Parameters: [Elements, 1 Elements]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "ArrayNew Completed Successfully with Result: 1 Elements",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Debug,
                    "Path MyDir was not fully qualified. Prepending the Current Directory: MyDir",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "PathCombine Completed Successfully with Result: string Length: 10",
                    null
                ),
                x =>
                {
                    x.LogLevel.Should().Be(LogLevel.Information);
                    x.Message.Should().BeOneOf(@"MyDir\File", @"MyDir/File");
                },
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Completed Successfully with Result: Unit",
                    null
                )
            )
            {
                FileSystemActions = new List<Action<Mock<IFileSystemHelper>>>
                {
                    x => x.Setup(f => f.GetCurrentDirectory()).Returns("MyDir")
                }
            };

            yield return new LoggingTestCase(
                "File Read",
                "FileRead 'MyFile' | Print",
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Started with Parameters: [Value, FileRead]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "FileRead Started with Parameters: [Path, \"MyFile\"], [Encoding, UTF8], [Decompress, False]",
                    null
                ),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "FileRead Completed Successfully with Result: UTF8-Stream",
                    null
                ),
                CheckMessageAndScope(LogLevel.Information, "MyData", null),
                CheckMessageAndScope(
                    LogLevel.Trace,
                    "Print Completed Successfully with Result: Unit",
                    null
                )
            )
            {
                FileSystemActions = new List<Action<Mock<IFileSystemHelper>>>
                {
                    x => x.Setup(a => a.ReadFile("MyFile", false))
                        .Returns(
                            () =>
                            {
                                var s  = new MemoryStream();
                                var sw = new StreamWriter(s);
                                sw.Write("MyData");
                                s.Seek(0, SeekOrigin.Begin);
                                sw.Flush();
                                return s;
                            }
                        )
                }
            };
        }
    }

    private static Action<LogEntry> CheckMessageAndScope(
        LogLevel logLevel,
        string expectedMessage,
        string? expectedScope)
    {
        return entry =>
        {
            entry.LogLevel.Should().Be(logLevel);
            entry.Message.Should().Be(expectedMessage);

            if (expectedScope != null
            ) //TODO make this parameter not optional once the bug in MELT is fixed
                entry.Scope.Message.Should().Be(expectedScope);
        };
    }

    public record LoggingTestCase : IAsyncTestInstance
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

        public List<Action<Mock<IFileSystemHelper>>>? FileSystemActions { get; set; }

        /// <inheritdoc />
        public async Task RunAsync(ITestOutputHelper testOutputHelper)
        {
            var spf = StepFactoryStore.CreateUsingReflection(typeof(IStep));

            var loggerFactory = TestLoggerFactory.Create();
            loggerFactory.AddXunit(testOutputHelper);

            var logger = loggerFactory.CreateLogger("Test");
            var repo   = new MockRepository(MockBehavior.Strict);

            var fileSystemMock = repo.Create<IFileSystemHelper>();

            if (FileSystemActions != null)
                foreach (var fileSystemAction in FileSystemActions)
                    fileSystemAction(fileSystemMock);

            var context = new ExternalContext(
                fileSystemMock.Object,
                repo.Create<IExternalProcessRunner>().Object,
                repo.Create<IConsole>().Object
            );

            var sclRunner = new SCLRunner(
                SCLSettings.EmptySettings,
                logger,
                spf,
                context
            );

            var r = await sclRunner.RunSequenceFromTextAsync(SCL, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.ToString()!);

            loggerFactory.Sink.LogEntries.Should().SatisfyRespectively(ExpectedLogs);
        }
    }
}

}
