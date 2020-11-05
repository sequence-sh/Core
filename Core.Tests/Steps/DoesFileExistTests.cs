using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DoesFileExistTests : StepTestBase<DoesFileExist, bool>
    {
        /// <inheritdoc />
        public DoesFileExistTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("File exists",
                        new DoesFileExist
                        {
                            Path = Constant("My Path")
                        }, true)
                    .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(true));

                yield return new StepCase("File does not exist",
                        new DoesFileExist
                        {
                            Path = Constant("My Path")
                        }, false)
                    .WithFileSystemAction(x => x.Setup(a => a.DoesFileExist("My Path")).Returns(false));
            }
        }

    }
}