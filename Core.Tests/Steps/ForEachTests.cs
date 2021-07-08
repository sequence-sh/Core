using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ForEachTests : StepTestBase<ForEach<int>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Default Variable Name",
                new ForEach<int>
                {
                    Action = new LambdaFunction<int, Unit>(
                        null,
                        new Log<int> { Value = GetVariable<int>(VariableName.Item) }
                    ),
                    Array = Array(3, 2, 1)
                },
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new StepCase(
                "Custom Variable Name",
                new ForEach<int>
                {
                    Action = new LambdaFunction<int, Unit>(
                        new VariableName("Foo"),
                        new Log<int> { Value = GetVariable<int>("Foo") }
                    ),
                    Array = Array(3, 2, 1),
                },
                Unit.Default,
                "3",
                "2",
                "1"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Default Variable Name",
                "Foreach [3,2,1]  (Log Value: <item>)",
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new DeserializeCase(
                "Named Variable",
                "Foreach [3,2,1] (Log Value: <Foo>) <Foo>",
                Unit.Default,
                "3",
                "2",
                "1"
            );

            yield return new DeserializeCase(
                "Scoped Variable Overloading",
                @"- Foreach [1,2,3] (Log <item>) #Here <item> is an int
- Foreach ['one', 'two','three'] (Log <item>) #Here <item> is a string
",
                Unit.Default,
                "1",
                "2",
                "3",
                "one",
                "two",
                "three"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Array Failure",
                new ForEach<int>
                {
                    Array = new FailStep<Array<int>> { ErrorMessage = "Array Failure" }
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Array Failure"
                )
            );

            yield return new ErrorCase(
                "Action Failure",
                new ForEach<int>
                {
                    Array = Array(1),
                    Action = new LambdaFunction<int, Unit>(
                        null,
                        new FailStep<Unit> { ErrorMessage = "Action Failure" }
                    ),
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Action Failure"
                )
            );
        }
    }
}

}
