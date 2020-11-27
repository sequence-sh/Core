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
                yield return new StepCase("Print file text",
                    new Print<string>
                    {
                        Value = new FromStream
                        {
                            Stream = new ReadFile
                            {
                                Path = Constant("File.txt"),
                            }
                        }
                    },
                    Unit.Default,
                    "Hello World"

                ).WithFileSystemAction(x=>x.Setup(a=>a.ReadFile("File.txt")).Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World"))));
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("ValueIf Error",
                        new ReadFile
                        {
                            Path =  Constant("File.txt"),
                        },
                        new ErrorBuilder("ValueIf Error", ErrorCode.Test)
                    )
                    .WithFileSystemAction(x => x.Setup(a => a.ReadFile(
                        "File.txt")).Returns(new ErrorBuilder("ValueIf Error", ErrorCode.Test)));
            }
        }
    }
}