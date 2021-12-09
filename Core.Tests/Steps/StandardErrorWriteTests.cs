using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StandardErrorWriteTests : StepTestBase<StandardErrorWrite, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Basic Test",
                        new StandardErrorWrite() { Data = StaticHelpers.Constant("Hello World") },
                        Unit.Default
                    )
                    .WithConsoleAction(
                        (x, mr) =>
                        {
                            var s = new MemoryStream();
                            x.Setup(c => c.OpenStandardError()).Returns(s);
                        }
                    )
                ;
        }
    }
}
