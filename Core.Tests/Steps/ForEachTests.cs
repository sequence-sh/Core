using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using Xunit.Sdk;
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
                yield return new StepCase("Simple Foreach",
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
                        "Simple Foreach",
                        "Foreach Action: (Print Value: <Foo>) Array: [3,2,1] Variable: <Foo>",
                        Unit.Default,
                        "3", "2", "1");
            }

        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                yield return new ErrorCase("Array Failure",
                    new ForEach<int>()
                    {
                        Array = new FailStep<Array<int>>() {ErrorMessage = "Array Failure"}
                    },
                    new SingleError("Array Failure", ErrorCode.Test, EntireSequenceLocation.Instance));

                yield return new ErrorCase("Action Failure",
                    new ForEach<int>()
                    {
                        Array = Array(1),
                        Action = new FailStep<Unit>() { ErrorMessage = "Action Failure" },
                        Variable = VariableName.Index
                    },
                    new SingleError("Action Failure", ErrorCode.Test, EntireSequenceLocation.Instance));
            }

        }
    }
}