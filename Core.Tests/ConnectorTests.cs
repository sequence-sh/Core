using System.IO;
using Sequence.ConnectorManagement.Base;
using Divergic.Logging.Xunit;
using Sequence.ConnectorManagement;

namespace Sequence.Core.Tests;

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

        var mockRepository = new MockRepository(MockBehavior.Strict);

        var stepFactoryStore = StepFactoryStore.TryCreate(
                mockRepository.OneOf<IExternalContext>(),
                new ConnectorData(ConnectorSettings.DefaultForAssembly(assembly), assembly)
            )
            .GetOrThrow();

        var injectedContextsResult = stepFactoryStore.TryGetInjectedContexts();

        injectedContextsResult.ShouldBeSuccessful();

        var externalContext = ExternalContext.Default with
        {
            InjectedContexts = injectedContextsResult.Value
        };

        var runner = new SCLRunner(
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
