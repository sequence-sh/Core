using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class SubtractTests : StepTestBase<Subtract, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "No numbers",
                new Subtract()
                {
                    Terms = new ArrayNew<int>() { Elements = new List<IStep<SCLInt>>() }
                },
                0
            );

            yield return new StepCase(
                "One number",
                new Subtract() { Terms = StaticHelpers.Array(2) },
                2
            );

            yield return new StepCase(
                "Two numbers",
                new Subtract() { Terms = StaticHelpers.Array(2, 3) },
                -1
            );

            yield return new StepCase(
                "Three numbers",
                new Subtract() { Terms = StaticHelpers.Array(2, 3, 4) },
                -5
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var (step, _) = CreateStepWithDefaultOrArbitraryValues();

            yield return new SerializeCase(
                "Default",
                step,
                @"0 - 1 - 2"
            );
        }
    }
}
