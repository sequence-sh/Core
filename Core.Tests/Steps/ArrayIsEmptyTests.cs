namespace Reductech.Sequence.Core.Tests.Steps;

public partial class ArrayIsEmptyTests : StepTestBase<ArrayIsEmpty<StringStream>, SCLBool>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "empty array",
                "ArrayIsEmpty Array: []",
                true.ConvertToSCLObject()
            );

            yield return new DeserializeCase(
                "two element",
                "ArrayIsEmpty Array: ['Hello','World']",
                false.ConvertToSCLObject()
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Empty",
                new ArrayIsEmpty<StringStream>()
                {
                    Array = new ArrayNew<StringStream>
                    {
                        Elements = new List<IStep<StringStream>>
                        {
                            Constant("Hello"), Constant("World"),
                        }
                    }
                },
                false.ConvertToSCLObject()
            );

            yield return new StepCase(
                "Not Empty",
                new ArrayIsEmpty<StringStream>()
                {
                    Array = new ArrayNew<StringStream>
                    {
                        Elements = new List<IStep<StringStream>>
                        {
                            Constant("Hello"), Constant("World"),
                        }
                    }
                },
                false.ConvertToSCLObject()
            );
        }
    }
}
