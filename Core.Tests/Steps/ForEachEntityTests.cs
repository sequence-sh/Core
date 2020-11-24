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
        public ForEachEntityTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("For each record. No line breaks",
                    new ForEachEntity
                    {
                        Action = new Print<Entity>
                        {
                            Value = GetVariable<Entity>(VariableName.Entity)
                        },EntityStream = Constant(EntityStream.Create(
                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))
                        ))
                    },
                    Unit.Default,
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                );


                //yield return new StepCase("For each record. Line breaks",
                //    new ForEachEntity
                //    {
                //        VariableName = new VariableName("Foo"),
                //        Action = new Print<Entity>
                //        {
                //            Value = GetVariable<Entity>("Foo")
                //        },
                //        EntityStream = Constant(EntityStream.Create(
                //            new Entity(new KeyValuePair<string, string>("Foo", "Hello"), new KeyValuePair<string, string>("Bar", "World")),
                //            new Entity(new KeyValuePair<string, string>("Foo", "Hello\n2"), new KeyValuePair<string, string>("Bar", "World\n2"))
                //        ))
                //    },
                //    Unit.Default,
                //    "Foo: Hello, Bar: World",
                //    "Foo: Hello\n2, Bar: World\n2"
                //).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello\n2"), new KeyValuePair<string, string>("Bar", "World\n2")));

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases {
            get
            {
                var expectedYaml = @"Do: ForEachEntity
Action: DoNothing()
EntityStream:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')";

                var (step, _) = CreateStepWithDefaultOrArbitraryValues();


                yield return new SerializeCase("Default", step, expectedYaml);
            } }
    }
}