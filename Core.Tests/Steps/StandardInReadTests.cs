using System.IO;
using System.Text;

namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StandardInReadTests : StepTestBase<StandardInRead, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Test",
                new StandardInRead(),
                "Hello World"
            ).WithConsoleAction(
                x => x.Setup(c => c.OpenStandardInput())
                    .Returns(new MemoryStream(Encoding.ASCII.GetBytes("Hello World")))
            );
        }
    }
}
