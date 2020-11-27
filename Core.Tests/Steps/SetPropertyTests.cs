using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SetPropertyTests : StepTestBase<EntitySetValue<string>, Entity>
    {
        /// <inheritdoc />
        public SetPropertyTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase(
                    "Set new property",
                    new EntitySetValue<string>
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"))),
                        Property = Constant("Bar"),
                        Value = Constant("World"),

                    },
                    CreateEntity(("Foo", "Hello"), ("Bar", "World")));

                yield return new StepCase(
                    "Change existing property",
                    new EntitySetValue<string>
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "Earth"))),
                        Property = Constant("Bar"),
                        Value = Constant("World"),

                    },
                    CreateEntity(("Foo", "Hello"), ("Bar", "World")));
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("default",
                    CreateStepWithDefaultOrArbitraryValues().step,
                    @"Do: EntitySetValue
Entity: (Prop1 = 'Val0',Prop2 = 'Val1')
Property: 'Bar2'
Value: 'Bar3'");
            }
        }
    }
}