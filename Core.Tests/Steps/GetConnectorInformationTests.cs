using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class
    GetConnectorInformationTests : StepTestBase<GetConnectorInformation, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var coreEntity = Entity.Create(("Name", "Reductech.EDR.Core"), ("Version", "0.6.0"));

            yield return new StepCase(
                "Core",
                new GetConnectorInformation(),
                new Array<Entity>(new[] { coreEntity })
            );
        }
    }
}

}
