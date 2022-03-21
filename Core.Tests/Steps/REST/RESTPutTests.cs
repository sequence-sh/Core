using System.Net;
using Reductech.Sequence.Core.Steps.REST;

namespace Reductech.Sequence.Core.Tests.Steps.REST;

public partial class RESTPutTests : StepTestBase<RESTPut, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTPut
                    {
                        BaseURL     = Constant("http://www.abc.com"),
                        RelativeURL = Constant("Thing/1"),
                        Entity      = Constant(Entity.Create(("a", 123)))
                    },
                    Unit.Default
                )
                .SetupHTTPSuccess(
                    "http://www.abc.com",
                    ("Thing/1", Method.Put, "{\"a\":123}"),
                    true,
                    HttpStatusCode.OK
                );
        }
    }
}
