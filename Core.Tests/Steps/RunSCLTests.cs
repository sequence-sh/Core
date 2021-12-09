using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class RunSCLTests : StepTestBase<RunSCL, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple Log",
                new RunSCL { SCL = Constant("Log 123") },
                Unit.Default,
                "123"
            );

            yield return new StepCase(
                    "Export Multiple Values",
                    new RunSCL
                    {
                        SCL = Constant(
                            @"
- <a> = 1
- <b> = 2
- <c> = 3"
                        ),
                        Export = Array("a", "b")
                    }, //Don't export c
                    Unit.Default
                )
                .WithExpectedFinalState("a", 1)
                .WithExpectedFinalState("b", 2);

            yield return new StepCase(
                "Export string value",
                new Sequence<Unit>
                {
                    InitialSteps =
                        new[]
                        {
                            new RunSCL
                            {
                                SCL = Constant("<myVar> = 'abc'"), Export = Array("myVar")
                            }
                        },
                    FinalStep = new Log<StringStream> { Value = GetVariable<StringStream>("myVar") }
                },
                Unit.Default,
                "abc"
            ).WithExpectedFinalState("myVar", "abc");

            yield return new StepCase(
                "Export int value",
                new Sequence<Unit>
                {
                    InitialSteps =
                        new[]
                        {
                            new RunSCL
                            {
                                SCL = Constant("<myVar> = 123"), Export = Array("myVar")
                            }
                        },
                    FinalStep = new Log<int> { Value = GetVariable<int>("myVar") }
                },
                Unit.Default,
                "123"
            ).WithExpectedFinalState("myVar", 123);
        }
    }
}
