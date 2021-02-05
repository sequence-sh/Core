using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class FileExistsTests : StepTestBase<FileExists, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "File exists",
                    new FileExists { Path = Constant("My Path") },
                    true
                )
                .WithFileAction(x => x.Setup(a => a.Exists("My Path")).Returns(true));

            yield return new StepCase(
                    "File does not exist",
                    new FileExists { Path = Constant("My Path") },
                    false
                )
                .WithFileAction(x => x.Setup(a => a.Exists("My Path")).Returns(false));
        }
    }
}

}
