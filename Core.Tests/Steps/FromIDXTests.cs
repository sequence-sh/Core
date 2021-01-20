using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FromIDXTests : StepTestBase<FromIDX, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new FromIDX
                {
                    Stream = StaticHelpers.Constant(
                        @"#DREREFERENCE abcd
#DREFIELD Foo= 1
#DREENDDOC
#DREENDDATAREFERENCE"
                    )
                },
                Entity.Create(("DREREFERENCE", "abcd"), ("Foo", "1"))
            );
        }
    }
}

}
