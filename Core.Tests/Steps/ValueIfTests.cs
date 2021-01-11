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

public class ValueIfTests : StepTestBase<ValueIf<int>, int>
{
    /// <inheritdoc />
    public ValueIfTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "ValueIf true",
                new ValueIf<int>()
                {
                    Condition = Constant(true), Then = Constant(1), Else = Constant(2)
                },
                1
            );

            yield return new StepCase(
                "ValueIf false",
                new ValueIf<int>()
                {
                    Condition = Constant(false), Then = Constant(1), Else = Constant(2)
                },
                2
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "ValueIf true",
                "ValueIf Condition: true Then: 1 Else: 2",
                1
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Condition is error",
                new ValueIf<Unit>()
                {
                    Condition = new FailStep<bool> { ErrorMessage = "Condition Fail" },
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(
                    EntireSequenceLocation.Instance,
                    ErrorCode_Core.Test,
                    "Condition Fail"
                )
            );

            yield return new ErrorCase(
                "Then is error",
                new ValueIf<Unit>()
                {
                    Condition = Constant(true),
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(EntireSequenceLocation.Instance, ErrorCode_Core.Test, "Then Fail")
            );

            yield return new ErrorCase(
                "Else is error",
                new ValueIf<Unit>()
                {
                    Condition = Constant(false),
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(EntireSequenceLocation.Instance, ErrorCode_Core.Test, "Else Fail")
            );
        }
    }
}

}
