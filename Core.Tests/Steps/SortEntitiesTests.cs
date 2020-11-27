using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SortEntitiesTests : StepTestBase<SortEntities, EntityStream >
    {
        /// <inheritdoc />
        public SortEntitiesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Ascending",
                    new EntityForEach
                    {
                        EntityStream = new SortEntities
                        {
                            Descending = Constant(true),
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Gamma")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            SortBy = new EntityGetValue {Entity = GetEntityVariable, Property = Constant("Foo")}
                        },
                        Action = new Print<Entity> {Value = GetEntityVariable}

                    }, Unit.Default,

                    "Foo: Alpha", "Foo: Beta", "Foo: Gamma"
                );

                    yield return new StepCase("Descending",
                    new EntityForEach
                    {
                        EntityStream = new SortEntities
                        {
                            Descending = Constant(false),
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Gamma")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            SortBy = new EntityGetValue { Entity = GetEntityVariable, Property = Constant("Foo") }
                        },
                        Action = new Print<Entity> { Value = GetEntityVariable }

                    }, Unit.Default,

                    "Foo: Gamma", "Foo: Beta", "Foo: Alpha"
                );


                yield return new StepCase("Missing Property",
                new EntityForEach
                {
                    EntityStream = new SortEntities
                    {
                        Descending = Constant(true),
                        EntityStream = new Constant<EntityStream>(EntityStream.Create(
                            CreateEntity(("Foo", "Gamma")),
                            CreateEntity(("Foo", "Alpha")),
                            CreateEntity(("Foo", "Beta")),
                            CreateEntity(("Bar", "Delta"))
                        )),
                        SortBy = new EntityGetValue { Entity = GetEntityVariable, Property = Constant("Foo") }
                    },
                    Action = new Print<Entity> { Value = GetEntityVariable }

                }, Unit.Default,

                "Bar: Delta", "Foo: Gamma", "Foo: Beta", "Foo: Alpha"
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
                    @"Do: SortEntities
EntityStream:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')
SortBy: 'Bar6'
Descending: True"

                    );

            }

        }
    }
}