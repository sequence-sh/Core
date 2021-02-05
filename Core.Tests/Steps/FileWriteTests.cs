using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Moq;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;
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
                .WithFileAction(
                    (x, mr) =>
                    {
                        var fs = mr.Create<IFileStream>();
                        //TODO setup
                        //fs.Setup(f=>f.writ .WriteAsync())

                        x.Setup(f => f.Create("Filename.txt"))
                            .Returns(fs.Object);
                    }
                );

            //.WithFileSystemAction(
            //    x => x.Setup(
            //            a =>
            //                a.WriteFileAsync(
            //                    "Filename.txt",
            //                    It.IsAny<Stream>(),
            //                    false,
            //                    It.IsAny<CancellationToken>()
            //                )
            //        )
            //        .ReturnsAsync(Unit.Default)
            //);

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
                .WithCompressionAction(
                    x => x.Setup(a => a.Compress(It.IsAny<IStream>()))
                        .Returns(
                            new StreamAdapter(
                                new MemoryStream(Encoding.ASCII.GetBytes("Hello World"))
                            )
                        )
                )
                .WithFileAction(
                    (x, mr) =>
                    {
                        var fs = mr.Create<IFileStream>();
                        //TODO setup
                        //fs.Setup(f=>f.writ .WriteAsync())

                        x.Setup(f => f.Create("Filename.txt"))
                            .Returns(fs.Object);
                    }
                );

            //TODO check that the text being sent is actually written
        }
    }
}

}
