using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class UnzipTests : StepTestBase<Unzip, Unit>
    {
        /// <inheritdoc />
        public UnzipTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Unzip",
                    new Unzip
                    {
                        ArchiveFilePath = Constant("Foo"),
                        DestinationDirectory = Constant("Bar"),
                        OverwriteFiles = Constant(true)
                    },
                    Unit.Default
                ).WithFileSystemAction(x=>x.Setup(a=>a.ExtractToDirectory("Foo", "Bar", true)).Returns(Unit.Default));
            }
        }


    }
}