using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityStreamConcatTests : StepTestBase<EntityStreamConcat, Sequence<Entity>>
    {
        /// <inheritdoc />
        public EntityStreamConcatTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("One stream",
                    new ForEach<Entity>
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        Array = new EntityStreamConcat
                        {
                            EntityStreams = new Array<Sequence<Entity>>
                            {
                                Elements = new List<IStep<Sequence<Entity>>>
                                {
                                    Array(CreateEntity(("Foo", "Alpha")), CreateEntity(("Foo", "Beta")))
                                }
                            }
                        },
                        Variable = VariableName.Entity

                    }, Unit.Default,

                    "(Foo: \"Alpha\")", "(Foo: \"Beta\")"
                );


                yield return new StepCase("Two streams",
                    new ForEach<Entity>
                    {
                        Action = new Print<Entity>{Value = GetEntityVariable},

                        Array = new EntityStreamConcat
                        {
                            EntityStreams = new Array<Sequence<Entity>>()
                            {
                                Elements = new List<IStep<Sequence<Entity>>>()
                                {
                                    Array(CreateEntity(("Foo", "Alpha")), CreateEntity(("Foo", "Beta"))),
                                    Array(CreateEntity(("Foo", "Gamma")), CreateEntity(("Foo", "Delta")))
                                }
                            }
                        },
                        Variable = VariableName.Entity

                    }, Unit.Default,

                    "(Foo: \"Alpha\")", "(Foo: \"Beta\")", "(Foo: \"Gamma\")", "(Foo: \"Delta\")"
                );

            }
        }
    }
}