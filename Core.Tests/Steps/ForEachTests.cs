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
    public class ForEachTests : StepTestBase<ForEach<int>, Unit>
    {
        /// <inheritdoc />
        public ForEachTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Default Variable Name",
                    new ForEach<int>
                    {
                        Action = new Print<int> { Value = GetVariable<int>(VariableName.Entity) },
                        Array = Array(3, 2, 1)
                    },
                    Unit.Default,
                    "3", "2", "1");

                yield return new StepCase("Custom Variable Name",
                    new ForEach<int>
                    {
                        Action = new Print<int> {Value = GetVariable<int>("Foo")},
                        Array = Array(3, 2, 1),
                        Variable = new VariableName("Foo")
                    },
                    Unit.Default,
                    "3", "2", "1");


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                yield return new DeserializeCase(
                        "Default Variable Name",
                        "Foreach [3,2,1]  (Print Value: <Entity>)",
                        Unit.Default,
                        "3", "2", "1");

                yield return new DeserializeCase(
                       "Named Variable",
                       "Foreach [3,2,1] (Print Value: <Foo>) <Foo>",
                       Unit.Default,
                       "3", "2", "1");

                yield return new DeserializeCase(
                    "Scoped Variable Overloading",
@"- Foreach [1,2,3] (Print <Entity>) #Here <Entity> is an int
- Foreach ['one', 'two','three'] (Print <Entity>) #Here <Entity> is a string
", Unit.Default, "1","2","3", "one", "two", "three"

                );
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Array Failure",
                    new ForEach<int>
                    {
                        Array = new FailStep<Array<int>> {ErrorMessage = "Array Failure"}
                    },
                    new SingleError("Array Failure", ErrorCode.Test, EntireSequenceLocation.Instance));

                yield return new ErrorCase("Action Failure",
                    new ForEach<int>
                    {
                        Array = Array(1),
                        Action = new FailStep<Unit> { ErrorMessage = "Action Failure" },
                        Variable = VariableName.Index
                    },
                    new SingleError("Action Failure", ErrorCode.Test, EntireSequenceLocation.Instance));
            }

        }
    }
}