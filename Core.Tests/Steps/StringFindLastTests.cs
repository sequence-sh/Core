using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class StringFindLastTests : StepTestBase<StringFindLast, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Substring present",
                new StringFindLast()
                {
                    String = Constant("Hello elle"), SubString = Constant("ell")
                },
                6
            );

            yield return new StepCase(
                "Substring not present",
                new StringFindLast()
                {
                    String = Constant("Hello elle"), SubString = Constant("ELL")
                },
                -1
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Substring present",
                "StringFindLast String: 'Hello ell' Substring: 'ell'",
                6
            );
        }
    }
}

}
