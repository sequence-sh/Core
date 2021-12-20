namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArrayFindTests : StepTestBase<ArrayFind<StringStream>, SCLInt>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple case",
                new ArrayFind<StringStream>()
                {
                    Array = Array(("Hello"), ("World")), Element = Constant("World")
                },
                1.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Duplicate Element",
                new ArrayFind<StringStream>
                {
                    Array = Array(("Hello"), ("World"), ("World")), Element = Constant("World")
                },
                1.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Element not present",
                new ArrayFind<StringStream>
                {
                    Array = Array(("Hello"), ("World"), ("World")), Element = Constant("Mark")
                },
                (-1).ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Simple Case",
                "ArrayFind Array: ['Hello', 'World'] Element: 'World'",
                1.ConvertToSCLObject()
            );
        }
    }
}
