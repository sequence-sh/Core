using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ValueIfTests : StepTestBase<ValueIf<int>, int>
    {
        /// <inheritdoc />
        public ValueIfTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("ValueIf true", new ValueIf<int>()
                {
                    Condition = Constant(true),
                    Then = Constant(1),
                    Else = Constant(2)
                }, 1 );

                yield return new StepCase("ValueIf false", new ValueIf<int>()
                {
                    Condition = Constant(false),
                    Then = Constant(1),
                    Else = Constant(2)
                }, 2);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("ValueIf true", "ValueIf(Condition: true, Then: 1, Else: 2)", 1);
            }

        }

    }
}