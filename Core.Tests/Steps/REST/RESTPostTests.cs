using System.Collections.Generic;
using System.Net;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using RestSharp;

namespace Reductech.EDR.Core.Tests.Steps.REST
{

public partial class RESTPostTests : StepTestBase<RESTPost, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTPost()
                    {
                        URL    = StaticHelpers.Constant("http://www.abc.com"),
                        Entity = StaticHelpers.Constant(Entity.Create(("a", 123)))
                    },
                    new StringStream("12345")
                )
                .SetupHTTPSuccess(
                    ("http://www.abc.com", Method.POST, "{\"a\":123}"),
                    HttpStatusCode.OK,
                    "12345"
                );
        }
    }
}

}
