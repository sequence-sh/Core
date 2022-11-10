namespace Sequence.Core.TestHarness.Rest;

public sealed class RESTClientSetupHelper
{
    private List<IRestSetup> Setups { get; } = new();

    public void AddHttpTestAction(IRestSetup restSetup) => Setups.Add(restSetup);

    public IRestClientFactory GetRESTClientFactory(MockRepository mockRepository)
    {
        return new TestRestClientFactory(mockRepository, Setups);
    }
}
