using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.ConnectorManagement.Base;
using Divergic.Logging.Xunit;
using Reductech.EDR.ConnectorManagement;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using RestSharp;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class ConnectorTests
{
    private static readonly string RelativePath = Path.Combine(
        "ExampleConnector",
        "bin",
        "ExampleConnector.dll"
    );

    [Fact]
    public async Task TestConnector()
    {
        var logger = new TestOutputLogger("Step Logger", TestOutputHelper);

        var absolutePath = PluginLoadContext.GetAbsolutePath(RelativePath);

        var assembly = PluginLoadContext.LoadPlugin(
            absolutePath,
            logger
        );

        Assert.NotNull(assembly);

        var stepTypes = assembly.GetTypes()
            .Where(x => typeof(IStep).IsAssignableFrom(x))
            .ToList();

        foreach (var type in stepTypes)
        {
            TestOutputHelper.WriteLine(type.Name);
        }

        var stepFactoryStore = StepFactoryStore.Create(
            new ConnectorData(ConnectorSettings.DefaultForAssembly(assembly), assembly)
        );

        var injectedContextsResult = stepFactoryStore.TryGetInjectedContexts();

        injectedContextsResult.ShouldBeSuccessful();

        var externalContext = ExternalContext.Default with
        {
            InjectedContexts = injectedContextsResult.Value
        };

        var runner = new SCLRunner(
            logger,
            stepFactoryStore,
            externalContext,
            DefaultRestClientFactory.Instance
        );

        var r = await
            runner.RunSequenceFromTextAsync(
                "Log (GetTestString)",
                new Dictionary<string, object>(),
                CancellationToken.None
            );

        r.ShouldBeSuccessful();
    }
}

}
