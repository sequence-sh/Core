using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SetPropertyTests : StepTestBase<SetProperty, Entity>
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
                    new SetProperty
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"))),
                        Property = Constant("Bar"),
                        Value = Constant("World" as object),

                    },
                    CreateEntity(("Foo", "Hello"), ("Bar", "World")));

                yield return new StepCase(
                    "Change existing property",
                    new SetProperty
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "Earth"))),
                        Property = Constant("Bar"),
                        Value = Constant("World" as object),

                    },
                    CreateEntity(("Foo", "Hello"), ("Bar", "World")));
            }
        }

    }
}