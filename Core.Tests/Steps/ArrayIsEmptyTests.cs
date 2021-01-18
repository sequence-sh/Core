using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayIsEmptyTests : StepTestBase<ArrayIsEmpty<StringStream>, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "empty array",
                "ArrayIsEmpty Array: []",
                true
            );

            yield return new DeserializeCase(
                "two element",
                "ArrayIsEmpty Array: ['Hello','World']",
                false
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
                false
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
                false
            );
        }
    }
}

}
