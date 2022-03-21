//namespace Reductech.Sequence.Core;

///// <summary>
///// Rest Client Factory that always returns a particular rest client
///// </summary>
//public class SingleRestClientFactory : IRestClientFactory
//{
//    /// <summary>
//    /// Returns a new SingleRestClientFactory
//    /// </summary>
//    public SingleRestClientFactory(IRestClient restClient)
//    {
//        RestClient = restClient;
//    }

//    /// <summary>
//    /// The Rest Client
//    /// </summary>
//    public IRestClient RestClient { get; }

//    /// <inheritdoc />
//    public IRestClient CreateRestClient(string baseUri)
//    {
//        if (!string.IsNullOrWhiteSpace(baseUri))
//            RestClient.BaseUrl = new Uri(baseUri);

//        return RestClient;
//    }
//}


