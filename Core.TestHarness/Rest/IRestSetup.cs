namespace Reductech.Sequence.Core.TestHarness.Rest;

public interface IRestSetup
{
    void SetupClient(Mock<IRestClient> client);
    string BaseUri { get; }
}
