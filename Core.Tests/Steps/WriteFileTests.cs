using System.Collections.Generic;
using System.IO;
using System.Threading;
using Moq;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class WriteFileTests : StepTestBase<WriteFile, Unit>
    {
        /// <inheritdoc />
        public WriteFileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Write file", new WriteFile
                    {
                        Path =  Constant("Filename.txt"),
                        Stream = new StringToStream{String = Constant("Hello World")}


                    },Unit.Default)
                    .WithFileSystemAction(x=>x.Setup(a=>
                        a.WriteFileAsync("Filename.txt",
                            It.IsAny<Stream>(), It.IsAny<CancellationToken>())).ReturnsAsync(Unit.Default));
                //TODO check that the text being sent is actually written
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return CreateDefaultSerializeCase(false);
            }
        }
    }
}