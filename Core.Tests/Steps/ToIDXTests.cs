using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToIDXTests : StepTestBase<ToIDX, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Single Property",
                new ToIDX() { Entity = Constant(Entity.Create(("Foo", 1))) },
                @"DREFIELD Foo= 1
#DREENDDOC
#DREENDDATAREFERENCE

"
            );
        }
    }
}

}
