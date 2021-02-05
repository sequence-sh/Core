using System.Collections.Generic;
using System.IO;
using System.Text;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Thinktecture.IO.Adapters;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ReadStandardInTests : StepTestBase<ReadStandardIn, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Test",
                new ReadStandardIn(),
                new StringStream("Hello World")
            ).WithConsoleAction(
                x => x.Setup(c => c.OpenStandardInput())
                    .Returns(
                        new StreamAdapter(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
                    )
            );
        }
    }
}

}
