using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class GetPropertyTests : StepTestBase<GetProperty, string>
    {
        /// <inheritdoc />
        public GetPropertyTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Get Simple Property",
                    new GetProperty
                    {
                        Entity =  Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        PropertyName = Constant("Foo")
                    },
                    "Hello");


                yield return new StepCase("Get Missing Property",
                    new GetProperty
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        PropertyName = Constant("Foot")
                    },
                    "");

                yield return new StepCase("Get Empty Property",
                    new GetProperty
                    {
                        Entity = Constant(CreateEntity(("Foo", ""), ("Bar", "World"))),
                        PropertyName = Constant("Foo")
                    },
                    "");

                yield return new StepCase("Get List Property",
                    new GetProperty
                    {
                        Entity = Constant(new Entity(new KeyValuePair<string, EntityValue>("Foo",
                            EntityValue.Create(new []{"Hello", "World"})
                            ))),
                        PropertyName = Constant("Foo")
                    },
                    "Hello,World");
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {

                var step = CreateStepWithDefaultOrArbitraryValues();

                yield return new SerializeCase("default",
                    step.step
                    ,
@"Do: GetProperty
Entity: (Prop1 = 'Val0',Prop2 = 'Val1')
PropertyName: 'Bar2'"

                    );
            }
        }
    }
}