using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class FirstIndexOfTests : StepTestBase<FirstIndexOf, int>
    {
        /// <inheritdoc />
        public FirstIndexOfTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Substring is present",
                    new FirstIndexOf()
                    {
                        String = Constant("Hello"),
                        SubString = Constant("lo")
                    }, 4
                );

                yield return new StepCase("Substring is no present",
                    new FirstIndexOf()
                    {
                        String = Constant("Hello"),
                        SubString = Constant("ol")
                    }, -1
                );

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Present", "FirstIndexOf(String = 'Hello', Substring = 'lo')", 4);

            }

        }

    }
}