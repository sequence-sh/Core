using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntitySetValueTests : StepTestBase<EntitySetValue<StringStream>, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Set new property",
                new EntitySetValue<StringStream>
                {
                    Entity   = Constant(CreateEntity(("Foo", "Hello"))),
                    Property = Constant("Bar"),
                    Value    = Constant("World"),
                },
                CreateEntity(("Foo", "Hello"), ("Bar", "World"))
            );

            yield return new StepCase(
                "Change existing property",
                new EntitySetValue<StringStream>
                {
                    Entity   = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "Earth"))),
                    Property = Constant("Bar"),
                    Value    = Constant("World"),
                },
                CreateEntity(("Foo", "Hello"), ("Bar", "World"))
            );
        }
    }

    //        /// <inheritdoc />
    //        protected override IEnumerable<SerializeCase> SerializeCases
    //        {
    //            get
    //            {
    //                yield return new SerializeCase("default",
    //                    CreateStepWithDefaultOrArbitraryValuesAsync().step,
    //                    @"Do: EntitySetValue
    //Entity: (Prop1 = 'Val0',Prop2 = 'Val1')
    //Property: 'Bar2'
    //Value: 'Bar3'");
    //            }
    //        }
}

}
