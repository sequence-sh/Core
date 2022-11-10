namespace Sequence.Core.Tests.Steps;

public partial class SetVariableTests : StepTestBase<SetVariable<SCLInt>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Set a new variable",
                new SetVariable<SCLInt>
                {
                    Value = Constant(42), Variable = new VariableName("Foo")
                },
                Unit.Default
            ).WithExpectedFinalState("Foo", 42);

            yield return new StepCase(
                    "Set an existing variable",
                    new Core.Steps.Sequence<Unit>
                    {
                        InitialSteps = new List<IStep<Unit>> { SetVariable("Foo", 21) },
                        FinalStep = new SetVariable<SCLInt>
                        {
                            Value = Constant(42), Variable = new VariableName("Foo")
                        }
                    },
                    Unit.Default
                )
                .WithExpectedFinalState("Foo", 42);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase("Set a new variable", "<Foo> = 42", Unit.Default)
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
                new SetVariable<SCLInt>
                {
                    Value = Constant(42), Variable = new VariableName("Foo")
                },
                "<Foo> = 42"
            );
        }
    }
}
