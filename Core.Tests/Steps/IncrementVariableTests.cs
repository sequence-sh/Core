using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class IncrementVariableTests : StepTestBase<IncrementVariable, Unit>
    {
        /// <inheritdoc />
        public IncrementVariableTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Increment 1",
                    new IncrementVariable()
                    {
                        Amount = Constant(1),
                        Variable = new VariableName("Foo")
                    },Unit.Default
                    ).WithInitialState("Foo", 41).WithExpectedFinalState("Foo", 42);

                yield return new StepCase("Increment 2",
                    new IncrementVariable()
                    {
                        Amount = Constant(2),
                        Variable = new VariableName("Foo")
                    }, Unit.Default
                    ).WithInitialState("Foo", 40).WithExpectedFinalState("Foo", 42);


                yield return new StepCase("Increment -1",
                    new IncrementVariable()
                    {
                        Amount = Constant(-1),
                        Variable = new VariableName("Foo")
                    }, Unit.Default
                    ).WithInitialState("Foo", 43).WithExpectedFinalState("Foo", 42);


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("increment 1", "IncrementVariable(Amount = 1, Variable = <Foo>)", Unit.Default)
                    .WithInitialState("Foo", 41).WithExpectedFinalState("Foo", 42);

            }

        }

    }
}