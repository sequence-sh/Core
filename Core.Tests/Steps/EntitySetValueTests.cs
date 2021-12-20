namespace Reductech.Sequence.Core.Tests.Steps;

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
                    Entity   = Constant(Entity.Create(("Foo", "Hello"))),
                    Property = Constant("Bar"),
                    Value    = Constant("World"),
                },
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
            );

            yield return new StepCase(
                "Change existing property",
                new EntitySetValue<StringStream>
                {
                    Entity   = Constant(Entity.Create(("Foo", "Hello"), ("Bar", "Earth"))),
                    Property = Constant("Bar"),
                    Value    = Constant("World"),
                },
                Entity.Create(("Foo", "Hello"), ("Bar", "World"))
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
