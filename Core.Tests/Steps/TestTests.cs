using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class TestTests : StepTestBase<Test<int>, int>
    {
        /// <inheritdoc />
        public TestTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Test true", new Test<int>()
                {
                    Condition = Constant(true),
                    ThenValue = Constant(1),
                    ElseValue = Constant(2)
                }, 1 );

                yield return new StepCase("Test false", new Test<int>()
                {
                    Condition = Constant(false),
                    ThenValue = Constant(1),
                    ElseValue = Constant(2)
                }, 2);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Test true", "Test(Condition = true, ThenValue = 1, ElseValue = 2)", 1);
            }

        }

    }
}