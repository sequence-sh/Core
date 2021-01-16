using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FindSubstringTests : StepTestBase<FindSubstring, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Substring is present",
                new FindSubstring() { String = Constant("Hello"), SubString = Constant("lo") },
                3
            );

            yield return new StepCase(
                "Substring is no present",
                new FindSubstring() { String = Constant("Hello"), SubString = Constant("ol") },
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
                "Present",
                "FindSubstring String: 'Hello' Substring: 'lo'",
                3
            );
        }
    }
}

}
