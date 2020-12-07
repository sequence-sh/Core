using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class SelectTests : StepTestBase<EntityMap, EntityStream>
    {
        /// <inheritdoc />
        public SelectTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Add property",
                    new EntityForEach
                    {
                        Action  = new Print<Entity> {Value = GetEntityVariable},

                        EntityStream = new EntityMap
                        {
                            EntityStream = Constant(EntityStream.Create(
                            CreateEntity(("Foo", "Hello")),
                            CreateEntity(("Foo", "Hello 2")))),

                            Function = new EntitySetValue<string>
                            {
                                Entity = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value = Constant("World")
                            }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\",Bar: \"World\")",
                    "(Foo: \"Hello 2\",Bar: \"World\")");

                yield return new StepCase("Change property",
                    new EntityForEach
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        EntityStream = new EntityMap
                        {
                            EntityStream = Constant(EntityStream.Create(
                            CreateEntity(("Foo", "Hello"), ("Bar", "Earth")),
                            CreateEntity(("Foo", "Hello 2"), ("Bar", "Earth")))),

                            Function = new EntitySetValue<string>
                            {
                                Entity = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value = Constant("World")
                            }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\",Bar: \"World\")",
                    "(Foo: \"Hello 2\",Bar: \"World\")");

            }
        }

    }
}