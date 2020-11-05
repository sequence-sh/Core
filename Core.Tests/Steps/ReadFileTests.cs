using System.Collections.Generic;
using System.IO;
using System.Text;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ReadFileTests : StepTestBase<ReadFile, Stream>
    {
        /// <inheritdoc />
        public ReadFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new SequenceStepCase("Print file text",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new Print<string>
                            {
                                Value = new FromStream
                                {
                                    Stream = new ReadFile
                                    {
                                        FileName = Constant("File.txt"),
                                        Folder = Constant("MyFolder")
                                    }
                                }
                            }
                        }
                    },
                    "Hello World"

                ).WithFileSystemAction(x=>x.Setup(a=>a.ReadFile(
                    Path.Combine("MyFolder", "File.txt"))).Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World"))));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Test Error",
                        new ReadFile
                        {
                            FileName = Constant("File.txt"),
                            Folder = Constant("MyFolder")
                        },
                        new ErrorBuilder("Test Error", ErrorCode.Test)
                    )
                    .WithFileSystemAction(x => x.Setup(a => a.ReadFile(
                        Path.Combine("MyFolder", "File.txt"))).Returns(new ErrorBuilder("Test Error", ErrorCode.Test)));
            }
        }
    }
}