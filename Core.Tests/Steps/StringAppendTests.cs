namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringAppendTests : StepTestBase<StringAppend, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases => CreateDefaultErrorCases();

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Append string to existing variable",
                    new Sequence<Unit>()
                    {
                        InitialSteps = new List<IStep<Unit>>()
                        {
                            new SetVariable<StringStream>()
                            {
                                Value    = Constant("Hello"),
                                Variable = new VariableName("Foo")
                            }
                        },
                        FinalStep = new StringAppend
                        {
                            Variable = new VariableName("Foo"), String = Constant("World")
                        }
                    },
                    Unit.Default
                )
                .WithExpectedFinalState("Foo", "HelloWorld");
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                    "Short Form",
                    "- <Foo> = 'Hello'\r\n- StringAppend String: 'World' Variable: <Foo>",
                    Unit.Default
                )
                .WithExpectedFinalState("Foo", "HelloWorld");
        }
    }
}
