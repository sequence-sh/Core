namespace Reductech.Sequence.Core.Tests.Steps;

public partial class GetVariableTests : StepTestBase<GetVariable<SCLInt>, SCLInt>
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
                    new SetVariable<SCLInt>
                    {
                        Variable = new VariableName("Foo"), Value = Constant(42)
                    },
                    new Log
                    {
                        Value = new GetVariable<SCLInt>
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
                    $"- <Foo> = 42\r\n- Log Value: <Foo>",
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
                new GetVariable<SCLInt>() { Variable = new VariableName("Foo") },
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
