using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SetVariableTests : StepTestBase<SetVariable<int>, Unit>
    {
        /// <inheritdoc />
        public SetVariableTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Set a new variable",
                    new SetVariable<int>
                    {
                        Value = Constant(42), VariableName = new VariableName("Foo")
                    }, Unit.Default
                    ).WithExpectedFinalState("Foo", 42);


                yield return new StepCase("Set an existing variable",
                    new SetVariable<int>
                    {
                        Value = Constant(42),
                        VariableName = new VariableName("Foo")
                    }, Unit.Default
                    )
                    .WithInitialState("Foo", 21)
                    .WithExpectedFinalState("Foo", 42);

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Set a new variable", "<Foo> = 42", Unit.Default)
                    .WithExpectedFinalState("Foo", 42);
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return new SerializeCase("Short form",
                    new SetVariable<int>
                    {
                        Value = Constant(42),
                        VariableName = new VariableName("Foo")
                    }, "<Foo> = 42"
                    );
            } }
    }
}