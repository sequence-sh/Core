﻿using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DistinctEntitiesTests : StepTestBase<EntityStreamDistinct, EntityStream>
    {
        /// <inheritdoc />
        public DistinctEntitiesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Distinct case sensitive",
                    new ForEachEntity
                    {
                        Action = new Print<Entity> {Value = GetEntityVariable},
                        EntityStream = new EntityStreamDistinct
                        {
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            KeySelector = new GetProperty() {Property = Constant("Foo"), Entity = GetEntityVariable}
                        }
                    },
                    Unit.Default,
                    "Foo: Alpha", "Foo: ALPHA", "Foo: Beta"
                );

                yield return new StepCase("Distinct case insensitive",
                    new ForEachEntity
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },
                        EntityStream = new EntityStreamDistinct
                        {
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            KeySelector = new GetProperty { Property = Constant("Foo"), Entity = GetEntityVariable },
                            IgnoreCase = Constant(true)
                        }
                    },
                    Unit.Default,
                    "Foo: Alpha",  "Foo: Beta"
                );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                yield return new SerializeCase("Default",
                    CreateStepWithDefaultOrArbitraryValues().step,
                    @"Do: DistinctEntities
EntityStream:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')
GetKey: 'Bar6'
CaseSensitive: True"

                    );

            }
        }
    }
}