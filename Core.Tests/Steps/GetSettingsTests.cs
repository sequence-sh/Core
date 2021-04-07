using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GetSettingsTests : StepTestBase<GetSettings, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Default Settings",
                new GetSettings(),
                Entity.Empty
            );

            var entity1 = Entity.Create(("a", 1), ("b", 2));

            yield return new StepCase(
                "Extra Settings",
                new GetSettings(),
                entity1
            ).WithSettings(new SCLSettings(entity1));
        }
    }
}

}
