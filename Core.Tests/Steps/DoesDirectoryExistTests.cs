using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DoesDirectoryExistTests : StepTestBase<DoesDirectoryExist, bool>
    {
        /// <inheritdoc />
        public DoesDirectoryExistTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Directory Exists",
                    new DoesDirectoryExist
                    {
                        Path = Constant("My Path")
                    },
                    true
                ).WithFileSystemAction(x=>x.Setup(a=>a.DoesDirectoryExist("My Path")).Returns(true));
            }
        }

    }
}