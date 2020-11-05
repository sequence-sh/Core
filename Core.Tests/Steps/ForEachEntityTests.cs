using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ForEachEntityTests : StepTestBase<ForEachEntity, Unit> //TODO sort out entity stream serialization
    {
        /// <inheritdoc />
        public ForEachEntityTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("For each record",
                    new ForEachEntity
                    {
                        VariableName = new VariableName("Foo"),
                        Action = new Print<Entity>
                        {
                            Value = GetVariable<Entity>("Foo")
                        },EntityStream = Constant(EntityStream.Create(
                            new Entity(new KeyValuePair<string, string>("Foo", "Hello"), new KeyValuePair<string, string>("Bar", "World")),
                            new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2"))
                        ))
                    },
                    Unit.Default,
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                ).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2")));
            }
        }
    }
}