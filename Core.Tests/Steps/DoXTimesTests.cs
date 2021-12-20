namespace Reductech.Sequence.Core.Tests.Steps;

public partial class DoXTimesTests : StepTestBase<DoXTimes, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log something three times",
                new DoXTimes { Action = new Log { Value = Constant(6) }, X = Constant(3) },
                Unit.Default,
                "6",
                "6",
                "6"
            );

            yield return new StepCase(
                "Run a sequence three times",
                new DoXTimes
                {
                    Action = new Core.Steps.Sequence<Unit>()
                    {
                        InitialSteps = new List<IStep<Unit>>()
                        {
                            new Log { Value = Constant(3) },
                        },
                        FinalStep = new Log { Value = Constant(3) },
                    },
                    X = Constant(3)
                },
                Unit.Default,
                "3",
                "3",
                "3",
                "3",
                "3",
                "3"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Log something three times",
                "DoXTimes Action: (Log Value: 6) X: 3",
                Unit.Default,
                "6",
                "6",
                "6"
            );
        }
    }
}
