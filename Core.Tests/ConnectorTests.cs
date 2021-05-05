using System.Collections.Generic;
using System.IO;
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
    private static readonly string RelativePath = Path.Combine(
        "ExampleConnector",
        "bin",
        "Debug",
        "net5.0",
        "ExampleConnector.dll"
    );

    //    [Fact]
    //    public async Task TestConnectorFromSettings()
    //    {
    //        var logger = new TestOutputLogger("Step Logger", TestOutputHelper);

    //        var connectorsString = $@"{{
    //""connectors"": {{
    //    ""example"":{{
    //        ""path"": {JsonConvert.SerializeObject(RelativePath)},
    //           ""ColorSource"": ""Red""

    //    }}
    //}}
    //}}";

    //        var settings = SCLSettings.CreateFromString(connectorsString);

    //        var stepFactoryStoreResult = StepFactoryStore.TryCreateFromSettings(settings, logger);

    //        stepFactoryStoreResult.ShouldBeSuccessful();

    //        var injectedContextsResult = stepFactoryStoreResult.Value.TryGetInjectedContexts(settings);

    //        injectedContextsResult.ShouldBeSuccessful();

    //        var externalContext = ExternalContext.Default with
    //        {
    //            InjectedContexts = injectedContextsResult.Value
    //        };

    //        var runner = new SCLRunner(
    //            SCLSettings.EmptySettings,
    //            logger,
    //            stepFactoryStoreResult.Value,
    //            externalContext
    //        );

    //        var r = await
    //            runner.RunSequenceFromTextAsync(
    //                "Log (GetTestString)",
    //                new Dictionary<string, object>(),
    //                CancellationToken.None
    //            );

    //        r.ShouldBeSuccessful();
    //    }

    [Fact]
    public async Task TestConnector()
    {
        var logger = new TestOutputLogger("Step Logger", TestOutputHelper);

        var absolutePath = PluginLoadContext.GetAbsolutePath(RelativePath);

        var assembly = PluginLoadContext.LoadPlugin(
            absolutePath,
            logger
        );

        assembly.ShouldBeSuccessful();

        var stepTypes = assembly.Value.GetTypes()
            .Where(x => typeof(IStep).IsAssignableFrom(x))
            .ToList();

        foreach (var type in stepTypes)
        {
            TestOutputHelper.WriteLine(type.Name);
        }

        var stepFactoryStore = StepFactoryStore.Create(
            new ConnectorData(ConnectorSettings.DefaultForAssembly(assembly.Value), assembly.Value)
        );

        var injectedContextsResult = stepFactoryStore.TryGetInjectedContexts(
            new SCLSettings(
                Entity.Create(
                    new List<(EntityPropertyKey key, object? value)>()
                    {
                        (new EntityPropertyKey(new[] { "connectors", "example", "colorSource" }),
                         "Red")
                    }
                )
            )
        );

        injectedContextsResult.ShouldBeSuccessful();

        var externalContext = ExternalContext.Default with
        {
            InjectedContexts = injectedContextsResult.Value
        };

        var runner = new SCLRunner(
            SCLSettings.EmptySettings,
            logger,
            stepFactoryStore,
            externalContext
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
