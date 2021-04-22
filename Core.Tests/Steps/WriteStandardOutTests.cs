using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class WriteStandardOutTests : StepTestBase<WriteStandardOut, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Basic Test",
                        new WriteStandardOut() { Data = StaticHelpers.Constant("Hello World") },
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

}
