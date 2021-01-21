using System.Collections.Generic;
using System.IO;
using System.Threading;
using Moq;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FileWriteTests : StepTestBase<FileWrite, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Write file",
                    new FileWrite
                    {
                        Path = Constant("Filename.txt"), Stream = Constant("Hello World")
                    },
                    Unit.Default
                )
                .WithFileSystemAction(
                    x => x.Setup(
                            a =>
                                a.WriteFileAsync(
                                    "Filename.txt",
                                    It.IsAny<Stream>(),
                                    false,
                                    It.IsAny<CancellationToken>()
                                )
                        )
                        .ReturnsAsync(Unit.Default)
                );

            yield return new StepCase(
                    "Write file compressed",
                    new FileWrite
                    {
                        Path     = Constant("Filename.txt"),
                        Stream   = Constant("Hello World"),
                        Compress = Constant(true)
                    },
                    Unit.Default
                )
                .WithFileSystemAction(
                    x => x.Setup(
                            a =>
                                a.WriteFileAsync(
                                    "Filename.txt",
                                    It.IsAny<Stream>(),
                                    true,
                                    It.IsAny<CancellationToken>()
                                )
                        )
                        .ReturnsAsync(Unit.Default)
                );

            //TODO check that the text being sent is actually written
        }
    }

    ///// <inheritdoc />
    //protected override IEnumerable<SerializeCase> SerializeCases
    //{
    //    get
    //    {
    //        yield return CreateDefaultSerializeCase(false);
    //    }
    //}
}

}
