using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FileMoveTests : StepTestBase<FileMove, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Move File",
                new FileMove()
                {
                    SourceFile      = Constant("MySource"),
                    DestinationFile = Constant("MyDestination")
                },
                Unit.Default
            ).WithFileAction(x => x.Setup(f => f.Move("MySource", "MyDestination")));
        }
    }
}

}
