using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityMapTests : StepTestBase<EntityMap, EntityStream>
    {
        /// <inheritdoc />
        public EntityMapTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

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

                            Function = new EntitySetValue<StringStream>
                            {
                                Entity = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value = Constant("World")
                            }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\" Bar: \"World\")",
                    "(Foo: \"Hello 2\" Bar: \"World\")");

                yield return new StepCase("Change property",
                    new EntityForEach
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },

                        EntityStream = new EntityMap
                        {
                            EntityStream = Constant(EntityStream.Create(
                            CreateEntity(("Foo", "Hello"), ("Bar", "Earth")),
                            CreateEntity(("Foo", "Hello 2"), ("Bar", "Earth")))),

                            Function = new EntitySetValue<StringStream>
                            {
                                Entity = GetEntityVariable,
                                Property = Constant("Bar"),
                                Value = Constant("World")
                            }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\" Bar: \"World\")",
                    "(Foo: \"Hello 2\" Bar: \"World\")");

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {

                //Do not do default cases as some errors are not propagated due to lazy evaluation

                yield return new ErrorCase("Stream error",
                    new EntityMap
                    {
                        EntityStream = new FailStep<EntityStream>{ ErrorMessage = "Stream Fail" },
                        Function = Constant(Entity.Create(("Key", "Value")))
                    },
                    new SingleError("Stream Fail", ErrorCode.Test, EntireSequenceLocation.Instance));
            }
        }

    }
}