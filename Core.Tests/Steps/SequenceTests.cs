namespace Reductech.EDR.Core.Tests.Steps;

public partial class SequenceTests : StepTestBase<Sequence<StringStream>, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No initial steps",
                new Sequence<StringStream> { FinalStep = Constant("Goodbye") },
                "Goodbye"
            );

            yield return new StepCase(
                "Sequence nested in final steps",
                new Sequence<StringStream>
                {
                    FinalStep = new Sequence<StringStream> { FinalStep = Constant("Goodbye") }
                },
                "Goodbye"
            );

            yield return new StepCase(
                "Sequence nested in initial steps",
                new Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new Sequence<Unit>
                        {
                            InitialSteps = new List<IStep<Unit>>
                            {
                                new Log<StringStream> { Value = Constant("Hello") }
                            },
                            FinalStep = new DoNothing()
                        }
                    },
                    FinalStep = Constant("Goodbye")
                },
                "Goodbye",
                "Hello"
            );

            yield return new StepCase(
                "Log then Log",
                new Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new Log<StringStream> { Value = Constant("Hello") },
                        new Log<StringStream> { Value = Constant("World") }
                    },
                    FinalStep = Constant("Goodbye")
                },
                "Goodbye",
                "Hello",
                "World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Log then Log",
                "- Log Value: 'Hello'\n- Log Value: 'World'",
                (Unit.Default),
                "Hello",
                "World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Short form",
                new Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new DoNothing(), new DoNothing(), new DoNothing()
                    },
                    FinalStep = Constant("Hello World")
                },
                $"- DoNothing{Environment.NewLine}- DoNothing{Environment.NewLine}- DoNothing{Environment.NewLine}- \"Hello World\"{Environment.NewLine}"
            );

            yield return new SerializeCase(
                "Final step is a compound step",
                new Sequence<StringStream>()
                {
                    FinalStep = new StringToCase()
                    {
                        Case   = Constant<TextCase>(TextCase.Upper),
                        String = Constant("Hello")
                    }
                },
                @"- StringToCase String: ""Hello"" Case: TextCase.Upper"
            );

            yield return new SerializeCase(
                "Sequence Nested in initial steps",
                new Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>>
                    {
                        new Sequence<Unit>
                        {
                            InitialSteps = new List<IStep<Unit>>
                            {
                                new Log<StringStream> { Value = Constant("Hello") }
                            },
                            FinalStep = new DoNothing()
                        }
                    },
                    FinalStep = Constant("Goodbye")
                },
                "\n- (\n\t- Log Value: \"Hello\"\n\t- DoNothing\n)\n- \"Goodbye\""
            );

            yield return new SerializeCase(
                "Sequence Nested in final steps",
                new Sequence<StringStream>
                {
                    FinalStep = new Sequence<StringStream> { FinalStep = Constant("Goodbye") }
                },
                "- (\n\t- \"Goodbye\"\n)"
            );

            yield return new SerializeCase(
                "Double nested Sequence",
                new Sequence<StringStream>
                {
                    FinalStep = new Sequence<StringStream>
                    {
                        FinalStep =
                            new Sequence<StringStream> { FinalStep = Constant("Goodbye") }
                    }
                },
                "- (\n\t- (\n\t\t- \"Goodbye\"\n\t)\n)"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Initial steps error",
                new Sequence<StringStream>
                {
                    InitialSteps =
                        new List<IStep<Unit>>
                        {
                            new FailStep<Unit> { ErrorMessage = "Initial step Fail" }
                        },
                    FinalStep = Constant("Final")
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Initial step Fail"
                )
            );

            yield return new ErrorCase(
                "Final steps error",
                new Sequence<StringStream>
                {
                    InitialSteps = new List<IStep<Unit>> { new DoNothing() },
                    FinalStep    = new FailStep<StringStream> { ErrorMessage = "Final step Fail" }
                },
                new SingleError(
                    ErrorLocation.EmptyLocation,
                    ErrorCode.Test,
                    "Final step Fail"
                )
            );
        }
    }
}
