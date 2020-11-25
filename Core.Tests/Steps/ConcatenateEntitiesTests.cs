using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ConcatenateEntitiesTests : StepTestBase<ConcatenateEntities, EntityStream>
    {
        /// <inheritdoc />
        public ConcatenateEntitiesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("One stream",
                    new ForEachEntity
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        EntityStream = new ConcatenateEntities
                        {
                            Streams = Array(
                                EntityStream.Create(
                                    CreateEntity(("Foo", "Alpha")),
                                    CreateEntity(("Foo", "Beta"))))
                        }

                    }, Unit.Default,

                    "Foo: Alpha", "Foo: Beta"
                );


                yield return new StepCase("Two streams, unordered",
                    new ForEachEntity
                    {
                        Action = new Print<Entity>{Value = GetEntityVariable},

                        EntityStream = new ConcatenateEntities
                        {
                            Streams = Array(
                                EntityStream.Create(
                                    CreateEntity(("Foo", "Alpha")),
                                    CreateEntity(("Foo", "Beta"))),
                                EntityStream.Create(
                                    CreateEntity(("Foo", "Gamma")),
                                    CreateEntity(("Foo", "Delta"))))
                        }

                    }, Unit.Default,

                    "Foo: Alpha", "Foo: Beta", "Foo: Gamma", "Foo: Delta"
                );


                yield return new StepCase("Two streams, ordered",
                    new ForEachEntity
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        EntityStream = new ConcatenateEntities
                        {
                            Streams = Array(
                                EntityStream.Create(
                                    CreateEntity(("Foo", "Alpha")),
                                    CreateEntity(("Foo", "Beta"))),
                                EntityStream.Create(
                                    CreateEntity(("Foo", "Gamma")),
                                    CreateEntity(("Foo", "Delta")))),

                            PreserveOrder = Constant(true)
                        }

                    }, Unit.Default,

                    "Foo: Alpha", "Foo: Beta", "Foo: Gamma", "Foo: Delta"
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
                    @"Do: ConcatenateEntities
Streams:
  Do: Array
  Elements:
  - - (Prop1 = 'Val0',Prop2 = 'Val1')
    - (Prop1 = 'Val2',Prop2 = 'Val3')
    - (Prop1 = 'Val4',Prop2 = 'Val5')
  - - (Prop1 = 'Val6',Prop2 = 'Val7')
    - (Prop1 = 'Val8',Prop2 = 'Val9')
    - (Prop1 = 'Val10',Prop2 = 'Val11')
  - - (Prop1 = 'Val12',Prop2 = 'Val13')
    - (Prop1 = 'Val14',Prop2 = 'Val15')
    - (Prop1 = 'Val16',Prop2 = 'Val17')
PreserveOrder: False"


                    );

            }
        }
    }
}