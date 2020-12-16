using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityStreamConcatTests : StepTestBase<EntityStreamConcat, IAsyncEnumerable<Entity>>
    {
        /// <inheritdoc />
        public EntityStreamConcatTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("One stream",
                    new EntityForEach
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        EntityStream = new EntityStreamConcat
                        {
                            EntityStreams = new Array<IAsyncEnumerable<Entity>>
                            {
                                Elements = new List<IStep<IAsyncEnumerable<Entity>>>
                                {
                                    Array(CreateEntity(("Foo", "Alpha")), CreateEntity(("Foo", "Beta")))
                                }
                            }
                        }

                    }, Unit.Default,

                    "(Foo: \"Alpha\")", "(Foo: \"Beta\")"
                );


                yield return new StepCase("Two streams",
                    new EntityForEach
                    {
                        Action = new Print<Entity>{Value = GetEntityVariable},

                        EntityStream = new EntityStreamConcat
                        {
                            EntityStreams = new Array<IAsyncEnumerable<Entity>>()
                            {
                                Elements = new List<IStep<IAsyncEnumerable<Entity>>>()
                                {
                                    Array(CreateEntity(("Foo", "Alpha")), CreateEntity(("Foo", "Beta"))),
                                    Array(CreateEntity(("Foo", "Gamma")), CreateEntity(("Foo", "Delta")))
                                }
                            }
                        }

                    }, Unit.Default,

                    "(Foo: \"Alpha\")", "(Foo: \"Beta\")", "(Foo: \"Gamma\")", "(Foo: \"Delta\")"
                );

            }
        }
    }
}