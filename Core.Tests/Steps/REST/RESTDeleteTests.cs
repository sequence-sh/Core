using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using Moq.RestSharp.Helpers;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using RestSharp;

namespace Reductech.EDR.Core.Tests.Steps.REST
{

public partial class RESTDeleteTests : StepTestBase<RESTDelete, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTDelete() { URL = StaticHelpers.Constant("http://www.abc.com/1") },
                    Unit.Default
                )
                .SetupHTTP(
                    request =>
                    {
                        request.Method.Should().Be(Method.DELETE);
                        request.Resource.Should().Be("http://www.abc.com/1");
                    },
                    x =>
                        x.MockApiResponse()
                            .WithStatusCode(HttpStatusCode.OK)
                            .MockExecuteAsync()
                );
        }
    }
}

}
