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
    public class WhereTests : StepTestBase<EntityStreamFilter, Array<Entity>>
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
                    new ForEach<Entity>
                    {
                        Action = new Print<Entity> { Value = GetEntityVariable },
                        Array = new EntityStreamFilter
                        {
                            EntityStream =  Array(
                                CreateEntity(("Foo", "Alpha")),
                                CreateEntity(("Bar", "Alpha")),
                                CreateEntity(("Foo", "ALPHA")),
                                CreateEntity(("Foo", "Beta")),
                                CreateEntity(("Bar", "Beta"))
                            ),
                            Predicate = new EntityHasProperty() { Property = Constant("Foo"), Entity = GetEntityVariable }
                        },
                        Variable = VariableName.Entity
                    },
                    Unit.Default,
                    "(Foo: \"Alpha\")", "(Foo: \"ALPHA\")", "(Foo: \"Beta\")");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {

                //Do not do default cases as some errors are not propagated due to lazy evaluation

                yield return new ErrorCase("EntityStream is error",
                    new EntityStreamFilter()
                    {
                        EntityStream = new FailStep<Array<Entity>>(){ErrorMessage = "Stream Fail"},
                        Predicate = Constant(true)
                    },
                    new SingleError("Stream Fail", ErrorCode.Test, EntireSequenceLocation.Instance));
            }
        }

    }
}