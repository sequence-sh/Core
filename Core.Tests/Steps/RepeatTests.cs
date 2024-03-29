﻿namespace Sequence.Core.Tests.Steps;

public partial class RepeatTests : StepTestBase<Repeat, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Log something three times",
                new Repeat { Action = new Log { Value = Constant(6) }, Times = Constant(3) },
                Unit.Default,
                "6",
                "6",
                "6"
            );

            yield return new StepCase(
                "Run a sequence three times",
                new Repeat
                {
                    Action = new Sequence<Unit>()
                    {
                        InitialSteps = new List<IStep<Unit>>()
                        {
                            new Log { Value = Constant(3) },
                        },
                        FinalStep = new Log { Value = Constant(3) },
                    },
                    Times = Constant(3)
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
                "Repeat Action: (Log Value: 6) Times: 3",
                Unit.Default,
                "6",
                "6",
                "6"
            );
        }
    }
}
