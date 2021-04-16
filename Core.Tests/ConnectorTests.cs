using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Xunit;

namespace Reductech.EDR.Core.Tests
{

[AutoTheory.UseTestOutputHelper]
public partial class ConnectorTests
{
    [Fact]
    public async Task TestConnectorFromSettings()
    {
        var logger = new TestOutputLogger("Step Logger", TestOutputHelper);

        var connectorsString = @"
{
""connectors"": {
    ""example"":{
        ""path"": ""ExampleConnector\\bin\\Debug\\net5.0\\ExampleConnector.dll""
        
    }
}
}";

        var settings = SCLSettings.CreateFromString(connectorsString);

        var stepFactoryStoreResult = StepFactoryStore.CreateFromSettings(settings, logger);

        stepFactoryStoreResult.ShouldBeSuccessful(x => x.AsString);

        var runner = new SCLRunner(
            SCLSettings.EmptySettings,
            logger,
            stepFactoryStoreResult.Value,
            ExternalContext.Default
        );

        var r = await
            runner.RunSequenceFromTextAsync(
                "Log (GetTestString)",
                new Dictionary<string, object>(),
                CancellationToken.None
            );

        r.ShouldBeSuccessful(x => x.AsString);
    }

    [Fact]
    public async Task TestConnector()
    {
        var logger = new TestOutputLogger("Step Logger", TestOutputHelper);

        var relativePath = @"ExampleConnector\bin\Debug\net5.0\ExampleConnector.dll";
        var absolutePath = PluginLoadContext.GetAbsolutePath(relativePath);

        var assembly = PluginLoadContext.LoadPlugin(
            absolutePath,
            logger
        );

        assembly.ShouldBeSuccessful(x => x.AsString);

        var stepTypes = assembly.Value.GetTypes()
            .Where(x => typeof(IStep).IsAssignableFrom(x))
            .ToList();

        foreach (var type in stepTypes)
        {
            TestOutputHelper.WriteLine(type.Name);
        }

        var stepFactoryStore = StepFactoryStore.CreateFromAssemblies(assembly.Value);

        var runner = new SCLRunner(
            SCLSettings.EmptySettings,
            logger,
            stepFactoryStore,
            ExternalContext.Default
        );

        var r = await
            runner.RunSequenceFromTextAsync(
                "Log (GetTestString)",
                new Dictionary<string, object>(),
                CancellationToken.None
            );

        r.ShouldBeSuccessful(x => x.AsString);
    }
}

}
