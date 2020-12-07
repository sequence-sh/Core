using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class MapFieldNamesTests : StepTestBase<EntityMapProperties, EntityStream>
    {
        /// <inheritdoc />
        public MapFieldNamesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Map some fields",

                    new EntityForEach
                    {
                        Action = new Print<Entity>
                        {
                            Value = GetVariable<Entity>(VariableName.Entity)
                        },
                        EntityStream =
                                    new EntityMapProperties
                                    {
                                        EntityStream = Constant(EntityStream.Create(
                                            CreateEntity(("Food", "Hello"),
                                                ("Bar", "World")),
                                            CreateEntity(("Food", "Hello 2"),
                                                ("Bar", "World 2")))),

                                        Mappings = new Constant<Entity>(CreateEntity(("Food", "Foo")))
                                    }
                    }, Unit.Default , "(Foo: \"Hello\",Bar: \"World\")",
                    "(Foo: \"Hello 2\",Bar: \"World 2\")"
                );
            }
        }
    }
}