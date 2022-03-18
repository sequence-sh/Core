using System.Text;
using Reductech.Sequence.Core.Steps.REST;

namespace Reductech.Sequence.Core.Tests.Steps.REST;

public partial class HttpRequestTests : StepTestBase<HttpRequest, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var basicCase = new StepCase(
                "Basic Case",
                new HttpRequest() { Uri = Constant("http://www.abc.com/Thing/1"), },
                new StringStream("abc")
            );

            basicCase
                .RESTClientSetupHelper.AddHttpTestAction(
                    new DownloadSetup("http://www.abc.com/Thing/1", "abc")
                );

            yield return basicCase;
        }
    }

    private record DownloadSetup(string Uri, string Data) : IRestSetup
    {
        /// <inheritdoc />
        public void SetupClient(Mock<IRestClient> client)
        {
            var byteArray = Encoding.ASCII.GetBytes(Data);

            client.SetupSet(x => x.BaseUrl = new Uri("http://www.abc.com/Thing/1"));

            client
                .Setup(x => x.DownloadData(It.Is<IRestRequest>(req => req.Method == Method.GET)))
                .Returns(byteArray);
        }
    }
}
