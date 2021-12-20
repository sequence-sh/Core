namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringContainsTests : StepTestBase<StringContains, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "True case sensitive",
                new StringContains
                {
                    String = Constant("Hello World"), Substring = Constant("Hello")
                },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "False case sensitive",
                new StringContains
                {
                    String = Constant("Hello World"), Substring = Constant("hello")
                },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "True case insensitive",
                new StringContains
                {
                    String     = Constant("Hello World"),
                    Substring  = Constant("hello"),
                    IgnoreCase = Constant(true)
                },
                true.ConvertToSCLObject()
            );

            yield return new StepCase(
                "False case insensitive",
                new StringContains
                {
                    String     = Constant("Hello World"),
                    Substring  = Constant("Goodbye"),
                    IgnoreCase = Constant(true)
                },
                false.ConvertToSCLObject()
            );
        }
    }
}
