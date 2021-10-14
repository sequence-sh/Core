using System;
using RestSharp;

namespace Reductech.EDR.Core
{

/// <summary>
/// Rest Client Factory that always returns a particular rest client
/// </summary>
public class SingleRestClientFactory : IRestClientFactory
{
    public SingleRestClientFactory(IRestClient restClient)
    {
        RestClient = restClient;
    }

    public IRestClient RestClient { get; }

    /// <inheritdoc />
    public IRestClient CreateRestClient(string baseUri)
    {
        if (!string.IsNullOrWhiteSpace(baseUri))
            RestClient.BaseUrl = new Uri(baseUri);

        return RestClient;
    }
}

}
