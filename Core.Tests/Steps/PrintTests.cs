using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class PrintTests : StepTestBase<Print<StringStream>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Print Something",
                new Print<StringStream> { Value = Constant("Hello") },
                Unit.Default
            ).WithConsoleAction(x => x.Setup(c => c.WriteLine("Hello")));
        }
    }
}

public partial class LogTests : StepTestBase<Log<StringStream>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log something",
                new Log<StringStream>() { Value = Constant("Hello") },
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
                "Log Value: 'Hello'",
                Unit.Default,
                "Hello"
            );

            yield return new DeserializeCase(
                "Ordered Argument",
                "Log 'Hello'",
                Unit.Default,
                "Hello"
            );
        }
    }
}

}
