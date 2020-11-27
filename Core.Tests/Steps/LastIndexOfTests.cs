using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class LastIndexOfTests : StepTestBase<FindLastSubstring, int>
    {
        /// <inheritdoc />
        public LastIndexOfTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Substring present",
                    new FindLastSubstring()
                    {
                        String = Constant("Hello elle"),
                        SubString = Constant("ell")
                    }, 6
                );

                yield return new StepCase("Substring not present",
                    new FindLastSubstring()
                    {
                        String = Constant("Hello elle"),
                        SubString = Constant("ELL")
                    }, -1
                );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Substring present", "FindLastSubstring(String = 'Hello ell', Substring = 'ell')", 6);

            }

        }

    }
}