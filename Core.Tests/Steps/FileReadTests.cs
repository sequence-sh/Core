using System.Collections.Generic;
using System.IO;
using System.Text;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class FileReadTests : StepTestBase<FileRead, StringStream>
{
    /// <inheritdoc />
    public FileReadTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Print file text",
                new Print<StringStream> { Value = new FileRead { Path = Constant("File.txt"), } },
                Unit.Default,
                "Hello World"
            ).WithFileSystemAction(
                x => x.Setup(a => a.ReadFile("File.txt"))
                    .Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Default",
                "Print Value: (FileRead Path: 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileSystemAction(
                x =>
                    x.Setup(a => a.ReadFile("File.txt"))
                        .Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
            );

            yield return new DeserializeCase(
                "Ordered Args",
                "Print (FileRead 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileSystemAction(
                x =>
                    x.Setup(a => a.ReadFile("File.txt"))
                        .Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
            );

            yield return new DeserializeCase(
                "Alias",
                "Print Value: (ReadFromFile Path: 'File.txt')",
                Unit.Default,
                "Hello World"
            ).WithFileSystemAction(
                x =>
                    x.Setup(a => a.ReadFile("File.txt"))
                        .Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                    "ValueIf Error",
                    new FileRead { Path = Constant("File.txt"), },
                    new ErrorBuilder_Core(ErrorCode_Core.Test, "ValueIf Error")
                )
                .WithFileSystemAction(
                    x => x.Setup(a => a.ReadFile("File.txt"))
                        .Returns(new ErrorBuilder_Core(ErrorCode_Core.Test, "ValueIf Error"))
                );
        }
    }
}

}
