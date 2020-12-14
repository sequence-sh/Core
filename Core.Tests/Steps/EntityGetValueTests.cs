using System.Collections.Generic;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityGetValueTests : StepTestBase<EntityGetValue, StringStream>
    {
        /// <inheritdoc />
        public EntityGetValueTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Get Simple Property",
                    new EntityGetValue
                    {
                        Entity =  Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        Property = Constant("Foo")
                    },
                    "Hello");


                yield return new StepCase("Get Missing Property",
                    new EntityGetValue
                    {
                        Entity = Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
                        Property = Constant("Foot")
                    },
                    "");

                yield return new StepCase("Get Empty Property",
                    new EntityGetValue
                    {
                        Entity = Constant(CreateEntity(("Foo", ""), ("Bar", "World"))),
                        Property = Constant("Foo")
                    },
                    "");

                yield return new StepCase("Get List Property",
                    new EntityGetValue
                    {
                        Entity = Constant(CreateEntity(("Foo", new []{"Hello", "World"}))),
                        Property = Constant("Foo")
                    },
                    "Hello,World");
            }
        }
    }
}