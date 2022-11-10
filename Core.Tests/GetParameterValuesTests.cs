using System.Collections.Immutable;
using Sequence.Core.Internal.Parser;

namespace Sequence.Core.Tests;

public class GetParameterValuesTests
{
    public GetParameterValuesTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; set; }

    [Fact]
    public void GetParameterValuesShouldReturnCorrectValues()
    {
        const string text = $"RunExternalProcess 'myPath' ['a', 'b', 'c']";
        var          sfs  = StepFactoryStore.Create();

        var parseResult = SCLParsing
            .TryParseStep(text)
            .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs, OptimizationSettings.None));

        parseResult.ShouldBeSuccessful();

        var pvs = parseResult.Value.GetParameterValues().ToList();

        var pathParameters = pvs.Where(x => x.Parameter.Metadata.ContainsKey("path")).ToList();

        pathParameters.Should().HaveCount(1);

        var value = pathParameters.Single()
            .Value.TryGetConstantValueAsync(
                ImmutableDictionary<VariableName, ISCLObject>.Empty,
                sfs
            )
            .Result;

        value.ShouldHaveValue();

        value
            .Value.Should()
            .Be(new StringStream("myPath"));
    }
}
