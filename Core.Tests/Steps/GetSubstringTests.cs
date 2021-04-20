using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GetSubstringTests : StepTestBase<GetSubstring, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Substring length 3",
                new GetSubstring
                {
                    String = Constant("Hello World"), Index = Constant(1), Length = Constant(3)
                },
                "ell"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "ArrayLength 3",
                "GetSubstring String: 'Hello World' Index: 1 Length: 3",
                "ell"
            );
        }
    }
}

}
