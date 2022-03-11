using Reductech.Sequence.Core.Internal.Parser;

namespace Reductech.Sequence.Core.Tests;

public class ConstantFoldingTests
{
    public ConstantFoldingTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; set; }

    [Theory]
    [InlineData("123",           123,   null)]
    [InlineData("123 + 456",     579,   null)]
    [InlineData("'abc'",         "abc", null)]
    [InlineData("<myVar>",       "abc", "abc")]
    [InlineData("123 + <myVar>", 579,   456)]
    public void GetParameterValuesShouldReturnCorrectValues(
        string scl,
        object expectedValue,
        object? myVarValue)

    {
        var sfs = StepFactoryStore.Create();

        var variables = myVarValue is null
            ? new Dictionary<VariableName, ISCLObject>()
            : new Dictionary<VariableName, ISCLObject>()
            {
                { new VariableName("myVar"), ISCLObject.CreateFromCSharpObject(myVarValue) }
            };

        var parseResult = SCLParsing
            .TryParseStep(scl)
            .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs, variables));

        parseResult.ShouldBeSuccessful();

        var expectedSclObject = ISCLObject.CreateFromCSharpObject(expectedValue);

        var cv = parseResult.Value.TryGetConstantValue(variables);
        cv.ShouldHaveValue();

        cv.Value.Should().Be(expectedSclObject);
    }
}
