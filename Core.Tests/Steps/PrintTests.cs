namespace Sequence.Core.Tests.Steps;

public partial class PrintTests : StepTestBase<Print, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Print Something",
                new Print { Value = Constant("Hello") },
                Unit.Default
            ).WithConsoleAction(x => x.Setup(c => c.WriteLine("Hello")));
        }
    }
}
