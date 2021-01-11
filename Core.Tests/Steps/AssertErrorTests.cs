using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class AssertErrorTests : StepTestBase<AssertError, Unit>
{
    /// <inheritdoc />
    public AssertErrorTests([NotNull] ITestOutputHelper testOutputHelper) :
        base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Print Divide by zero",
                "AssertError Step: (Print Value: (1 / 0))",
                Unit.Default,
                "ApplyMathOperator Failed with message: Attempt to Divide by Zero.",
                "Print Failed with message: Attempt to Divide by Zero."
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Print divide by zero",
                new AssertError
                {
                    Step = new Print<int>
                    {
                        Value = new ApplyMathOperator()
                        {
                            Left     = Constant(1),
                            Operator = Constant(MathOperator.Divide),
                            Right    = Constant(0),
                        }
                    }
                },
                Unit.Default,
                "ApplyMathOperator Failed with message: Attempt to Divide by Zero.",
                "Print Failed with message: Attempt to Divide by Zero."
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Successful Step",
                new AssertError
                {
                    Step = new Print<StringStream> { Value = Constant("Hello World") }
                },
                new ErrorBuilder(ErrorCode_Core.AssertionFailed, Constant("Print").Name)
            );
        }
    }
}

}
