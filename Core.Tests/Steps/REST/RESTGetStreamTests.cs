using System.Collections.Generic;
using System.Net;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using RestSharp;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps.REST
{

public partial class RESTGetStreamTests : StepTestBase<RESTGetStream, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTGetStream()
                    {
                        BaseURL     = Constant("http://www.abc.com"),
                        RelativeURL = Constant("Thing/1")
                    },
                    new StringStream("abc")
                )
                .SetupHTTPSuccess(
                    "http://www.abc.com",
                    ("Thing/1", Method.GET, null),
                    HttpStatusCode.OK,
                    "abc"
                );
        }
    }
}

}
