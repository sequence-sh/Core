using System.Net;
using Reductech.Sequence.Core.Steps.REST;

namespace Reductech.Sequence.Core.Tests.Steps.REST;

public partial class RESTDeleteTests : StepTestBase<RESTDelete, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                    "Basic Case",
                    new RESTDelete()
                    {
                        BaseURL     = Constant("http://www.abc.com"),
                        RelativeURL = Constant("Thing/1")
                    },
                    Unit.Default
                )
                .SetupHTTPSuccess(
                    "http://www.abc.com",
                    ("Thing/1", Method.DELETE, null),
                    HttpStatusCode.OK
                );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Request Failure",
                new RESTDelete()
                {
                    BaseURL = Constant("http://www.abc.com"), RelativeURL = Constant("Thing/1")
                },
                ErrorCode.RequestFailed.ToErrorBuilder(
                    HttpStatusCode.Forbidden,
                    "Test Forbidden",
                    "Test Error"
                )
            ).SetupHTTPError(
                "http://www.abc.com",
                ("Thing/1", Method.DELETE, null),
                HttpStatusCode.Forbidden,
                "Test Forbidden",
                "Test Error"
            );

            foreach (var errorCase in base.ErrorCases)
            {
                yield return errorCase;
            }
        }
    }
}
