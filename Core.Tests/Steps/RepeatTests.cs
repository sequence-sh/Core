using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class RepeatTests : StepTestBase<Repeat<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Repeat number",
                new Repeat<int>() { Element = Constant(6), Number = Constant(3) },
                ArrayHelper.ToArray(new List<int>() { 6, 6, 6 })
            );

            yield return new StepCase(
                "Repeat zero times",
                new Repeat<int>() { Element = Constant(6), Number = Constant(0) },
                ArrayHelper.ToArray(new List<int>())
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Repeat number",
                "Repeat Element: 6 Number: 3",
                ArrayHelper.ToArray(new List<int>() { 6, 6, 6 })
            );
        }
    }
}

}
