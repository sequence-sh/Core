using RestSharp;

namespace Reductech.EDR.Core
{

/// <summary>
/// For Creating Rest Clients
/// </summary>
public interface IRestClientFactory
{
    /// <summary>
    /// Create a new RestClient
    /// </summary>
    IRestClient CreateRestClient(string baseUri);
}

}
