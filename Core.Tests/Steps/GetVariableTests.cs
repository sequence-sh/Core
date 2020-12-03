using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetVariableTests : StepTestBase<GetVariable<int>, int>
    {
        /// <inheritdoc />
        public GetVariableTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                var sequence = new Sequence<Unit>
                {
                    Steps = new List<IStep<Unit>>
                    {
                        new SetVariable<int>
                        {
                            Variable = new VariableName("Foo"),
                            Value = Constant(42)
                        },
                        new Print<int>
                        {
                            Value = new GetVariable<int>
                            {
                                Variable = new VariableName("Foo")
                            }
                        }
                    },
                    FinalStep = new DoNothing()
                };



                yield return new StepCase("Get Variable", sequence, Unit.Default,  "42").WithExpectedFinalState("Foo", 42);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {

                yield return new DeserializeCase("Short Form",
                    $"- <Foo> = 42{Environment.NewLine}- Print(Value = <Foo>)",
                    Unit.Default, "42"
                    ).WithInitialState("Foo", 42)
                    .WithExpectedFinalState("Foo", 42);

            }

        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                yield return new SerializeCase("Short form", new GetVariable<int>(){Variable = new VariableName("Foo")}, "<Foo>");
            } }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get { yield return CreateDefaultErrorCase(false); }
        }
    }
}