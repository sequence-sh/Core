using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EntityStreamDistinctTests : StepTestBase<EntityStreamDistinct, AsyncList<Entity>>
    {
        /// <inheritdoc />
        public EntityStreamDistinctTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Distinct case sensitive",
                    new ForEach<Entity>
                    {
                        Action = new Print<Entity> {Value = GetEntityVariable},
                        Array = new EntityStreamDistinct
                        {
                            EntityStream = Array(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            ),
                            KeySelector = new EntityGetValue() {Property = Constant("Foo"), Entity = GetEntityVariable}
                        },
                        Variable = VariableName.Entity
                    },
                    Unit.Default,
                    "(Foo: \"Alpha\")", "(Foo: \"ALPHA\")", "(Foo: \"Beta\")"
                );

                yield return new StepCase("Distinct case insensitive",
                    new ForEach<Entity>
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },
                        Array = new EntityStreamDistinct
                        {
                            EntityStream = Array(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Foo", "Beta"))
                            ),
                            KeySelector = new EntityGetValue { Property = Constant("Foo"), Entity = GetEntityVariable },
                            IgnoreCase = Constant(true)
                        },
                        Variable = VariableName.Entity
                    },
                    Unit.Default,
                    "(Foo: \"Alpha\")",  "(Foo: \"Beta\")"
                );
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {

                //Do not do default cases as some errors are not propagated due to lazy evaluation

                yield return new ErrorCase("Stream is error",
                    new EntityStreamDistinct()
                    {
                        EntityStream = new FailStep<AsyncList<Entity>>() { ErrorMessage = "Stream Fail" },
                        KeySelector =  Constant("A")
                    },
                    new SingleError("Stream Fail", ErrorCode.Test, EntireSequenceLocation.Instance));
            }
        }
    }
}