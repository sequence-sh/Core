namespace Sequence.Core.Tests.Steps;

public partial class IncrementVariableTests : StepTestBase<IncrementVariable, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Increment 1",
                new Sequence<Unit>()
                {
                    InitialSteps = new List<IStep<Unit>>() { SetVariable("Foo", 41) },
                    FinalStep = new IncrementVariable()
                    {
                        Amount = Constant(1), Variable = new VariableName("Foo")
                    }
                },
                Unit.Default
            ).WithExpectedFinalState("Foo", 42);

            yield return new StepCase(
                "Increment 2",
                new Sequence<Unit>()
                {
                    InitialSteps = new List<IStep<Unit>>() { SetVariable("Foo", 40) },
                    FinalStep = new IncrementVariable()
                    {
                        Amount = Constant(2), Variable = new VariableName("Foo")
                    }
                },
                Unit.Default
            ).WithExpectedFinalState("Foo", 42);

            yield return new StepCase(
                "Increment -1",
                new Sequence<Unit>
                {
                    InitialSteps = new List<IStep<Unit>> { SetVariable("Foo", 43) },
                    FinalStep = new IncrementVariable
                    {
                        Amount = Constant(-1), Variable = new VariableName("Foo")
                    }
                },
                Unit.Default
            ).WithExpectedFinalState("Foo", 42);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "increment 1",
                    "- <Foo> = 41\r\n- IncrementVariable Amount: 1 Variable: <Foo>",
                    Unit.Default
                )
                .WithExpectedFinalState("Foo", 42);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases =>
        CreateDefaultErrorCases(1.ConvertToSCLObject());
}
