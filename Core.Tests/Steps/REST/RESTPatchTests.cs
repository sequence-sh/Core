using System.Net;
using Reductech.EDR.Core.Steps.REST;

namespace Reductech.EDR.Core.Tests.Steps.REST;

public partial class RESTPatchTests : StepTestBase<RESTPatch, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTPatch()
                    {
                        BaseURL     = Constant("http://www.abc.com"),
                        RelativeURL = Constant("Thing/1"),
                        Entity      = Constant(Entity.Create(("a", 123)))
                    },
                    Unit.Default
                )
                .SetupHTTPSuccess(
                    "http://www.abc.com",
                    ("Thing/1", Method.PATCH, "{\"a\":123}"),
                    HttpStatusCode.OK
                );
        }
    }
}
