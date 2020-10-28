using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ForEachTests : StepTestBase<ForEach<int>, Unit>
    {
        /// <inheritdoc />
        public ForEachTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Simple Foreach",
                    new ForEach<int>
                    {
                        Action = new Print<int>{Value = GetVariable<int>("Foo")},
                        Array = Array(3,2,1),
                        VariableName = new VariableName("Foo")
                    },
                    Unit.Default,
                    3,2,1)
                    .WithExpectedFinalState("Foo", 1);

                yield return new StepCase("No elements",
                    new ForEach<int>
                    {
                        Action = new Print<int> { Value = GetVariable<int>("Foo") },
                        Array = new Array<int>{Elements = new List<IStep<int>>()},
                        VariableName = new VariableName("Foo")
                    },
                    Unit.Default);


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase("Simple Foreach", "Foreach(Action = Print(Value = <Foo>), Array = [3,2,1], VariableName = <Foo>)", Unit.Default, 3,2,1)
                    .WithExpectedFinalState("Foo", 1);
            }

        }

    }
}