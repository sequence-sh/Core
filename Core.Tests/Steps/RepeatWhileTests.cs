using System.Collections.Generic;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class RepeatWhileTests : StepTestBase<While, Unit>
{
    /// <inheritdoc />
    public RepeatWhileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Repeat while Foo < 5",
                new Core.Steps.Sequence<Unit>
                {
                    InitialSteps = new List<IStep<Unit>> { SetVariable("Foo", 1) },
                    FinalStep = new While()
                    {
                        Action = new IncrementVariable
                        {
                            Amount = Constant(1), Variable = new VariableName("Foo")
                        },
                        Condition = new Compare<int>
                        {
                            Left     = GetVariable<int>("Foo"),
                            Right    = Constant(5),
                            Operator = Constant(CompareOperator.LessThan)
                        }
                    }
                },
                Unit.Default
            ).WithExpectedFinalState("Foo", 5);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Repeat while Foo < 5",
                    "- <Foo> = 1 \r\n- While Condition: (<Foo> < 5) Action: (IncrementVariable Variable: <Foo> Amount: 1)",
                    Unit.Default
                )
                .WithExpectedFinalState("Foo", 5);
        }
    }
}

}
