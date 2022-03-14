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
    [InlineData("123",                       123,               null)]
    [InlineData("123 + 456",                 579,               null)]
    [InlineData("'abc'",                     "abc",             null)]
    [InlineData("<myVar>",                   "abc",             "abc")]
    [InlineData("123 + <myVar>",             579,               456)]
    [InlineData("arrayMap [1,2,3] (<> + 1)", new[] { 2, 3, 4 }, null)]
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

        var expectedSclObject = ISCLObject.CreateFromCSharpObject(expectedValue)
            .Serialize(SerializeOptions.Primitive);

        var cv = parseResult.Value.TryGetConstantValueAsync(variables, sfs).Result;
        cv.ShouldHaveValue();

        cv.Value.Serialize(SerializeOptions.Primitive).Should().Be(expectedSclObject);
    }
}
