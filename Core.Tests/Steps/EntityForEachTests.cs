//using System.Collections.Generic;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Steps;
//using Reductech.EDR.Core.TestHarness;
//using Reductech.EDR.Core.Util;
//using Xunit.Abstractions;
//using static Reductech.EDR.Core.TestHarness.StaticHelpers;

//namespace Reductech.EDR.Core.Tests.Steps
//{
//    public partial class EntityForEachTests : StepTestBase<EntityForEach, Unit>
//    {
//        /// <inheritdoc />
//        public EntityForEachTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

//        /// <inheritdoc />
//        protected override IEnumerable<StepCase> StepCases
//        {
//            get
//            {
//                yield return new StepCase("For each record. No line breaks",
//                    new EntityForEach
//                    {
//                        Action = new Print<Entity>
//                        {
//                            Value = GetVariable<Entity>(VariableName.Entity)
//                        },EntityStream = Array(
//                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
//                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2")))
//                    },
//                    Unit.Default,
//                    "(Foo: \"Hello\" Bar: \"World\")",
//                    "(Foo: \"Hello 2\" Bar: \"World 2\")"
//                );

//                //yield return new StepCase("For each record. Line breaks",
//                //    new ForEachEntity
//                //    {
//                //        Variable = new Variable("Foo"),
//                //        Action = new Print<Entity>
//                //        {
//                //            Value = GetVariable<Entity>("Foo")
//                //        },
//                //        EntityStream = Constant(EntityStream.Create(
//                //            new Entity(new KeyValuePair<string, string>("Foo", "Hello"), new KeyValuePair<string, string>("Bar", "World")),
//                //            new Entity(new KeyValuePair<string, string>("Foo", "Hello\n2"), new KeyValuePair<string, string>("Bar", "World\n2"))
//                //        ))
//                //    },
//                //    Unit.Default,
//                //    "Foo: Hello, Bar: World",
//                //    "Foo: Hello\n2, Bar: World\n2"
//                //).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello\n2"), new KeyValuePair<string, string>("Bar", "World\n2")));

//            }
//        }
//    }
//}


