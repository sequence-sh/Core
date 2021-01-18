using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToJsonTests : StepTestBase<ToJson, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Json",
                new ToJson() { Entities = Array(Entity.Create(("Foo", 1))) },
                "[{\"Foo\":1}]"
            );
        }
    }
}

}
