using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class DistinctEntitiesTests : StepTestBase<DistinctEntities, EntityStream>
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
                        EntityStream = new DistinctEntities
                        {
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            DistinctBy = new GetProperty() {PropertyName = Constant("Foo"), Entity = GetEntityVariable}
                        }
                    },
                    Unit.Default,
                    "Foo: Alpha", "Foo: ALPHA", "Foo: Beta"
                );

                yield return new StepCase("Distinct case insensitive",
                    new ForEachEntity
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },
                        EntityStream = new DistinctEntities
                        {
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            )),
                            DistinctBy = new GetProperty { PropertyName = Constant("Foo"), Entity = GetEntityVariable },
                            CaseSensitive = Constant(false)
                        }
                    },
                    Unit.Default,
                    "Foo: Alpha", "Foo: ALPHA", "Foo: Beta"
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
- (Prop1 = 'Val1',Prop2 = 'Val2')
- (Prop1 = 'Val3',Prop2 = 'Val4')
- (Prop1 = 'Val5',Prop2 = 'Val6')
DistinctBy: 'Bar0'
CaseSensitive: True"

                    );

            }
        }
    }
}