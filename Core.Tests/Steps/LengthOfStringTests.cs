using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class LengthOfStringTests : StepTestBase<LengthOfString, int>
    {
        /// <inheritdoc />
        public LengthOfStringTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("simple length of string",
                    new LengthOfString()
                    {
                        String = Constant("Hello")
                    }, 5
                    );


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Simple length of string", "LengthOfString(String = 'Hello')", 5);

            }

        }

    }
}