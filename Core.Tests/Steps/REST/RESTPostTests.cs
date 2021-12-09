using System.Collections.Generic;
using System.Net;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using RestSharp;

namespace Reductech.EDR.Core.Tests.Steps.REST;

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
                        BaseURL     = Constant("http://www.abc.com"),
                        RelativeURL = Constant("Thing/1"),
                        Entity      = Constant(Entity.Create(("a", 123)))
                    },
                    new StringStream("12345")
                )
                .SetupHTTPSuccess(
                    "http://www.abc.com",
                    ("Thing/1", Method.POST, "{\"a\":123}"),
                    HttpStatusCode.OK,
                    "12345"
                );
        }
    }
}
