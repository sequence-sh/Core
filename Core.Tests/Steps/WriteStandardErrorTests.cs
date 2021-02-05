using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture.IO.Adapters;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class WriteStandardErrorTests : StepTestBase<WriteStandardError, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                        "Basic Test",
                        new WriteStandardError() { Data = StaticHelpers.Constant("Hello World") },
                        Unit.Default
                    )
                    .WithConsoleAction(
                        (x, mr) =>
                        {
                            var s = new MemoryStream();
                            x.Setup(c => c.OpenStandardError()).Returns(new StreamAdapter(s));
                        }
                    )
                ;
        }
    }
}

}
