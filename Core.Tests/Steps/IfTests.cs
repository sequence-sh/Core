using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class IfTests : StepTestBase<If, Unit>
{
    /// <inheritdoc />
    public IfTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "If true print something",
                "If Condition: true Then: (Print Value: 'Hello World')",
                Unit.Default,
                "Hello World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "If true print something",
                new If
                {
                    Condition = Constant(true),
                    Then      = new Print<StringStream>() { Value = Constant("Hello World") }
                },
                Unit.Default,
                "Hello World"
            );

            yield return new StepCase(
                "If false print nothing",
                new If
                {
                    Condition = Constant(false),
                    Then      = new Print<StringStream> { Value = Constant("Hello World") }
                },
                Unit.Default
            );

            yield return new StepCase(
                "If false print something else",
                new If
                {
                    Condition = Constant(false),
                    Then      = new Print<StringStream> { Value = Constant("Hello World") },
                    Else      = new Print<StringStream> { Value = Constant("Goodbye World") },
                },
                Unit.Default,
                "Goodbye World"
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
                new If()
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
                new If()
                {
                    Condition = Constant(true),
                    Then      = new FailStep<Unit> { ErrorMessage = "Then Fail" },
                    Else      = new FailStep<Unit> { ErrorMessage = "Else Fail" },
                },
                new SingleError(EntireSequenceLocation.Instance, ErrorCode_Core.Test, "Then Fail")
            );

            yield return new ErrorCase(
                "Else is error",
                new If()
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
