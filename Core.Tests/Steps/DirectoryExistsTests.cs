using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class DirectoryExistsTests : StepTestBase<DirectoryExists, bool>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Directory Exists",
                new DirectoryExists { Path = Constant("My Path") },
                true
            ).WithFileSystemAction(
                x => x.Setup(a => a.DoesDirectoryExist("My Path")).Returns(true)
            );
        }
    }
}

}
