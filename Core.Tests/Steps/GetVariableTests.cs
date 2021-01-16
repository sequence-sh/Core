using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GetVariableTests : StepTestBase<GetVariable<int>, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var sequence = new Core.Steps.Sequence<Unit>
            {
                InitialSteps = new List<IStep<Unit>>
                {
                    new SetVariable<int>
                    {
                        Variable = new VariableName("Foo"), Value = Constant(42)
                    },
                    new Print<int>
                    {
                        Value = new GetVariable<int>
                        {
                            Variable = new VariableName("Foo")
                        }
                    }
                },
                FinalStep = new DoNothing()
            };

            yield return new StepCase("Get Variable", sequence, Unit.Default, "42")
                .WithExpectedFinalState("Foo", 42);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Short Form",
                    $"- <Foo> = 42\r\n- Print Value: <Foo>",
                    (Unit.Default),
                    "42"
                )
                .WithExpectedFinalState("Foo", 42);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Short form",
                new GetVariable<int>() { Variable = new VariableName("Foo") },
                "<Foo>"
            );
        }
    }

    ///// <inheritdoc />
    //protected override IEnumerable<ErrorCase> ErrorCases
    //{
    //    get { yield return CreateDefaultErrorCase(false); }
    //}
}

}
