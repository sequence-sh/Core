using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityStreamCreateTests : StepTestBase<EntityStreamCreate, EntityStream>
    {
        /// <inheritdoc />
        public EntityStreamCreateTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Basic Entity Stream",
                    new EntityStreamCreate()
                    {
                        Elements = new List<IStep<Entity>>()
                        {
                            new Constant<Entity>(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                            new Constant<Entity>(CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))),
                        }
                    },
                    new EntityStream(new List<Entity>()
                    {
                        CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                        CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2")),

                    }.ToAsyncEnumerable())


                );
            }
        }

    }
}