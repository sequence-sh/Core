//using System.Collections.Generic;
//using System.Linq;
//using Reductech.EDR.Core.Entities;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Steps;
//using Reductech.EDR.Core.TestHarness;
//using Xunit.Abstractions;
//using static Reductech.EDR.Core.TestHarness.StaticHelpers;

//namespace Reductech.EDR.Core.Tests.Steps
//{
//    public class EntityStreamCreateTests : StepTestBase<Entitystr, AsyncList<Entity>>
//    {
//        /// <inheritdoc />
//        public EntityStreamCreateTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//        {
//        }

//        /// <inheritdoc />
//        protected override IEnumerable<StepCase> StepCases
//        {
//            get
//            {
//                yield return new StepCase("Basic Entity Stream",
//                    new EntityStreamCreate()
//                    {
//                        Elements = new List<IStep<Entity>>()
//                        {
//                             Constant(CreateEntity(("Foo", "Hello"), ("Bar", "World"))),
//                            Constant(CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))),
//                        }
//                    },
//                    new EntityStream(new List<Entity>()
//                    {
//                        CreateEntity(("Foo", "Hello"), ("Bar", "World")),
//                        CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2")),

//                    }.ToAsyncEnumerable())

//                );
//            }
//        }

//    }
//}


