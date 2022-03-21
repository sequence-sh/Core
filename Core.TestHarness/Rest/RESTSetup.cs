using System.Linq.Expressions;

namespace Reductech.Sequence.Core.TestHarness.Rest;

public sealed class RESTSetup : IRestSetup
{
    public RESTSetup(
        string baseUri,
        Expression<Func<RestRequest, bool>> checkRequest,
        RestResponse response)
    {
        BaseUri      = baseUri;
        CheckRequest = checkRequest;
        Response     = response;
    }

    public string BaseUri { get; }
    public Expression<Func<RestRequest, bool>> CheckRequest { get; }

    public RestResponse Response { get; }

    public void SetupClient(Mock<IRestClient> client)
    {
        //if (!string.IsNullOrWhiteSpace(BaseUri))
        //{
        //    var uri = new Uri(BaseUri);
        //    client.SetupSet<Uri>(x => x.BaseUrl = uri);
        //}

        client
            .Setup(
                s => s.ExecuteAsync(
                    It.Is(CheckRequest),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<RestRequest, CancellationToken>(
                (request, _) =>
                {
                    Response.Request = request;
                }
            )
            .ReturnsAsync(Response);
    }
}
