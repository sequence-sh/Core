using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class WhereTests : StepTestBase<EntityStreamFilter, EntityStream>
    {
        /// <inheritdoc />
        public WhereTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Filter stuff",
                    new EntityForEach
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },
                        EntityStream = new EntityStreamFilter
                        {
                            EntityStream = new Constant<EntityStream>(EntityStream.Create(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Bar", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Bar", "Beta"))
                            )),
                            Predicate = new EntityHasProperty() { Property = Constant("Foo"), Entity = GetEntityVariable }
                        }
                    },
                    Unit.Default,
                    "(Foo: \"Alpha\")", "(Foo: \"ALPHA\")", "(Foo: \"Beta\")");
            }
        }

    }
}