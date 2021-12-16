using System.IO;
using Divergic.Logging.Xunit;
using Reductech.EDR.Core.Internal.Parser;

namespace Reductech.EDR.Core.Tests;

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
[AutoTheory.UseTestOutputHelper]
public partial class ExampleTests
{
    public const string SkipString = "skip";

    #pragma warning disable xUnit1004 // Test methods should not be skipped
    [Theory(Skip = SkipString)]
    #pragma warning restore xUnit1004 // Test methods should not be skipped
    [Trait("Category", "Integration")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\Sort.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\EntityMapProperties.scl")]
    [InlineData(@"C:\Users\wainw\source\repos\Reductech\edr\Examples\ChangeCase.scl")]
    public async Task RunSCLFromFile(string path)
    {
        var scl = await File.ReadAllTextAsync(path);

        TestOutputHelper.WriteLine(scl);

        var sfs = StepFactoryStore.Create();

        var stepResult = SCLParsing.TryParseStep(scl)
            .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs));

        if (stepResult.IsFailure)
            throw new XunitException(
                string.Join(
                    ", ",
                    stepResult.Error.GetAllErrors()
                        .Select(x => x.Message + " " + x.Location.AsString())
                )
            );

        var monad = new StateMonad(
            TestOutputHelper.BuildLogger(),
            sfs,
            ExternalContext.Default,
            new Dictionary<string, object>()
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful();
    }

    private const string DocumentationCreateExample = @"
- <root> = 'Documentation'
- <docs> = DocumentationCreate

# Create a directory for each connector
- <docs>
  | ArrayDistinct (From <item> 'Directory')
  | ForEach (
      CreateDirectory (PathCombine [<root>, (From <item> 'Directory')])
    )

# Export all steps to .\<root>\ConnectorName\StepName.md
- <docs>
  | Foreach (
      FileWrite
        (From <item> 'FileText')
        (PathCombine [<root>, (From <item> 'Directory'), (From <item> 'FileName')])
    )";

    private const string RESTGetExample = @"
Log (RestGETStream 'https://en.wikipedia.org/api/rest_v1/page/pdf/Edgar_Allan_Poe')
";

    [Theory(Skip = SkipString)]
    //[Theory()]
    [InlineData(DocumentationCreateExample)]
    [InlineData(RESTGetExample)]
    [Trait("Category", "Integration")]
    public async Task RunSCLSequence(string scl)
    {
        var logger =
            TestOutputHelper.BuildLogger(new LoggingConfig() { LogLevel = LogLevel.Information });

        var sfs = StepFactoryStore.Create();

        var runner = new SCLRunner(
            logger,
            sfs,
            ExternalContext.Default
        );

        var r = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        r.ShouldBeSuccessful();
    }
}
