namespace Reductech.Sequence.Core.TestHarness.Rest;

public class TestRestClientFactory : IRestClientFactory
{
    public TestRestClientFactory(MockRepository repository, IEnumerable<IRestSetup> setups)
    {
        _clients =
            setups.GroupBy(x => x.BaseUri)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var clientMock = repository.Create<IRestClient>(MockBehavior.Strict);

                        foreach (var restSetup in g)
                        {
                            restSetup.SetupClient(clientMock);
                        }

                        return clientMock.Object;
                    }
                );
    }

    private readonly IReadOnlyDictionary<string, IRestClient> _clients;

    /// <inheritdoc />
    public IRestClient CreateRestClient(string baseUri)
    {
        return _clients[baseUri];
    }
}
