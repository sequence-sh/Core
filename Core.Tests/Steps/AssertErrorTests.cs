using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class AssertErrorTests : StepTestBase<AssertError, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Log Divide by zero",
                "AssertError Step: (Log Value: (1 / 0))",
                Unit.Default,
                "Divide Failed with message: Attempt to Divide by Zero.",
                "Log Failed with message: Attempt to Divide by Zero."
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log divide by zero",
                new AssertError
                {
                    Step = new Log<int> { Value = new Divide() { Terms = Array(1, 0) } }
                },
                Unit.Default,
                "Divide Failed with message: Attempt to Divide by Zero.",
                "Log Failed with message: Attempt to Divide by Zero."
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
                    Step = new Log<StringStream> { Value = Constant("Hello World") }
                },
                new ErrorBuilder(ErrorCode.AssertionFailed, Constant("Log").Name)
            );
        }
    }
}

}
