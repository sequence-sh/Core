﻿using System.Collections.Generic;
using System.Net;
using Reductech.EDR.Core.Steps.REST;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
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

}