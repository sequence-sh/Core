using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class PrintTests : StepTestBase<Print<StringStream>, Unit>
{
    /// <inheritdoc />
    public PrintTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Print something",
                new Print<StringStream>() { Value = Constant("Hello") },
                Unit.Default,
                "Hello"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Named argument",
                "Print Value: 'Hello'",
                Unit.Default,
                "Hello"
            );

            yield return new DeserializeCase(
                "Log Alias",
                "Log Value: 'Hello'",
                Unit.Default,
                "Hello"
            );

            yield return new DeserializeCase(
                "Ordered Argument",
                "Print 'Hello'",
                Unit.Default,
                "Hello"
            );
        }
    }
}

}
