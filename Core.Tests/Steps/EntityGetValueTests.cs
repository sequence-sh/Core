﻿using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityGetValueTests : StepTestBase<EntityGetValue, string>
    {
        /// <inheritdoc />
        public EntityGetValueTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Get Simple Property",
                    new EntityGetValue
                    {
                        Entity =  Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        Property = Constant("Foo")
                    },
                    "Hello");


                yield return new StepCase("Get Missing Property",
                    new EntityGetValue
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        Property = Constant("Foot")
                    },
                    "");

                yield return new StepCase("Get Empty Property",
                    new EntityGetValue
                    {
                        Entity = Constant(CreateEntity(("Foo", ""), ("Bar", "World"))),
                        Property = Constant("Foo")
                    },
                    "");

                yield return new StepCase("Get List Property",
                    new EntityGetValue
                    {
                        Entity = Constant(new Entity(new KeyValuePair<string, EntityValue>("Foo",
                            EntityValue.Create(new []{"Hello", "World"})
                            ))),
                        Property = Constant("Foo")
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
@"Do: EntityGetValue
Entity: (Prop1 = 'Val0',Prop2 = 'Val1')
Property: 'Bar2'"

                    );
            }
        }
    }
}