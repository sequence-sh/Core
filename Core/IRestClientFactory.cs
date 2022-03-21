using System.IO;

namespace Reductech.Sequence.Core;

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

/// <summary>
/// A rest client
/// </summary>
public interface IRestClient
{
    /// <summary>
    /// Execute a Rest Request Asynchronously
    /// </summary>
    Task<RestResponse> ExecuteAsync(
        RestRequest request,
        CancellationToken cancellationToken);

    /// <summary>A specialized method to download files as streams.</summary>
    Task<Stream?> DownloadStreamAsync(
        RestRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// Uses an Underlying RestClient
/// </summary>
public class DefaultRestClient : IRestClient
{
    /// <summary>
    /// Create a DefaultRestClient
    /// </summary>
    public DefaultRestClient(RestClient restClient)
    {
        RestClient = restClient;
    }

    /// <summary>
    /// The RestClient
    /// </summary>
    public RestClient RestClient { get; }

    /// <inheritdoc />
    public Task<RestResponse> ExecuteAsync(
        RestRequest request,
        CancellationToken cancellationToken)
    {
        return RestClient.ExecuteAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream?> DownloadStreamAsync(
        RestRequest request,
        CancellationToken cancellationToken)
    {
        return RestClient.DownloadStreamAsync(request, cancellationToken);
    }
}
