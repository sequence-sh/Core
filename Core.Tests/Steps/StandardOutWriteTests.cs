using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StandardOutWriteTests : StepTestBase<StandardOutWrite, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Basic Test",
                        new StandardOutWrite() { Data = StaticHelpers.Constant("Hello World") },
                        Unit.Default
                    )
                    .WithConsoleAction(
                        (x, mr) =>
                        {
                            var s = new MemoryStream();
                            x.Setup(c => c.OpenStandardOutput()).Returns(s);
                        }
                    )
                ;
        }
    }
}
