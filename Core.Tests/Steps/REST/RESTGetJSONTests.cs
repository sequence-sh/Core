using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Moq.RestSharp.Helpers;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using RestSharp;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps.REST
{

public partial class RESTGetJSONTests : StepTestBase<RESTGetJSON, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTGetJSON() { URL = Constant("http://www.abc.com") },
                    Entity.Create(("a", 1))
                )
                .SetupHTTP(
                    request =>
                    {
                        request.Method.Should().Be(Method.GET);
                        request.Resource.Should().Be("http://www.abc.com");
                    },
                    x =>
                        x.MockApiResponse()
                            .WithStatusCode(HttpStatusCode.OK)
                            .ReturnsJsonString("{\"a\": 1}")
                            .MockExecuteAsync()
                );
        }
    }
}

}
