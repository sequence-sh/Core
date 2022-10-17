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
            ? new Dictionary<VariableName, InjectedVariable>()
            : new Dictionary<VariableName, InjectedVariable>()
            {
                {
                    new VariableName("myVar"),
                    new InjectedVariable(ISCLObject.CreateFromCSharpObject(myVarValue), null)
                }
            };

        var parseResult = SCLParsing
            .TryParseStep(scl)
            .Bind(
                x => x.TryFreeze(
                    SCLRunner.RootCallerMetadata,
                    sfs,
                    new OptimizationSettings(true, variables)
                )
            );

        parseResult.ShouldBeSuccessful();

        var expectedSclObject = ISCLObject.CreateFromCSharpObject(expectedValue)
            .Serialize(SerializeOptions.Primitive);

        var cv = parseResult.Value.TryGetConstantValueAsync(
                variables.ToDictionary(x => x.Key, x => x.Value.SCLObject),
                sfs
            )
            .Result;

        cv.ShouldHaveValue();

        cv.Value.Serialize(SerializeOptions.Primitive).Should().Be(expectedSclObject);
    }

    [Theory]
    [InlineData("log 123",            null)]
    [InlineData("- <q> = 1\r\n- <q>", null)]
    public async Task UnfoldableStepShouldNotFold(string scl, object? myVarValue)
    {
        var sfs = StepFactoryStore.Create();

        var variables = myVarValue is null
            ? new Dictionary<VariableName, InjectedVariable>()
            : new Dictionary<VariableName, InjectedVariable>()
            {
                {
                    new VariableName("myVar"),
                    new InjectedVariable(ISCLObject.CreateFromCSharpObject(myVarValue), null)
                }
            };

        var parseResult = SCLParsing
            .TryParseStep(scl)
            .Bind(
                x => x.TryFreeze(
                    SCLRunner.RootCallerMetadata,
                    sfs,
                    new OptimizationSettings(true, variables)
                )
            );

        parseResult.ShouldBeSuccessful();

        parseResult.Value.HasConstantValue(variables.Keys).Should().BeFalse();

        var r = await parseResult.Value.TryGetConstantValueAsync(
            variables.ToDictionary(x => x.Key, x => x.Value.SCLObject),
            sfs
        );

        r.ShouldHaveNoValue();
    }
}
