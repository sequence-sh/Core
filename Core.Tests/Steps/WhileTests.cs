using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class WhileTests : StepTestBase<While, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Repeat while Foo < 5",
                new Sequence<Unit>
                {
                    InitialSteps = new List<IStep<Unit>> { SetVariable("Foo", 1) },
                    FinalStep = new While
                    {
                        Action = new IncrementVariable
                        {
                            Amount = Constant(1), Variable = new VariableName("Foo")
                        },
                        Condition = new LessThan<int>
                        {
                            Terms = new ArrayNew<int>
                            {
                                Elements = new[]
                                {
                                    GetVariable<int>("Foo"), Constant(5)
                                }
                            }
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
