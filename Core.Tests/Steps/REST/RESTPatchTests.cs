using System.Collections.Generic;
using System.Net;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using RestSharp;

namespace Reductech.EDR.Core.Tests.Steps.REST
{

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
                        URL    = StaticHelpers.Constant("http://www.abc.com"),
                        Entity = StaticHelpers.Constant(Entity.Create(("a", 123)))
                    },
                    Unit.Default
                )
                .SetupHTTPSuccess(
                    ("http://www.abc.com", Method.PATCH, "{\"a\":123}"),
                    HttpStatusCode.OK
                );
        }
    }
}

}
