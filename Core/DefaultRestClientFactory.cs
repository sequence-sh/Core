﻿using RestSharp;

namespace Reductech.EDR.Core
{

/// <summary>
/// Uses a standard REST client
/// </summary>
public class DefaultRestClientFactory : IRestClientFactory
{
    private DefaultRestClientFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IRestClientFactory Instance { get; } = new DefaultRestClientFactory();

    /// <inheritdoc />
    public IRestClient CreateRestClient(string baseUri)
    {
        return new RestClient(baseUri);
    }
}

}