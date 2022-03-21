using System.IO;
using System.Text;
using Reductech.Sequence.Core.Steps.REST;
using Reductech.Sequence.Core.TestHarness.Rest;

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

    private record DownloadSetup(string BaseUri, string Data) : IRestSetup
    {
        /// <inheritdoc />
        public void SetupClient(Mock<IRestClient> client)
        {
            var byteArray = Encoding.ASCII.GetBytes(Data);
            var stream    = new MemoryStream(byteArray);

            client
                .Setup(
                    x => x.DownloadStreamAsync(
                        It.Is<RestRequest>(req => req.Method == Method.Get),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(stream);
        }
    }
}
