using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class HasPropertyTests : StepTestBase<HasProperty, bool>
    {
        /// <inheritdoc />
        public HasPropertyTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Property exists",
                    new HasProperty
                    {
                        Property = Constant("Foo"),
                        Entity = Constant(CreateEntity(("Foo", "Hello")))
                    }, true);

                yield return new StepCase("Property missing",
                    new HasProperty
                    {
                        Property = Constant("Bar"),
                        Entity = Constant(CreateEntity(("Hello", "World")))
                    }, false);
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("default",
                    CreateStepWithDefaultOrArbitraryValues().step,
                    @"Do: HasProperty
Entity: (Prop1 = 'Val0',Prop2 = 'Val1')
Property: 'Bar2'");
            }
        }

    }
}