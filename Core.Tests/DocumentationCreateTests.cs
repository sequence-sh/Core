using Reductech.Sequence.Core.Internal.Documentation;

namespace Reductech.Sequence.Core.Tests;

public class DocumentationCreateTests
{
    public DocumentationCreateTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
        StepFactoryStore = StepFactoryStore.Create();
    }

    private ITestOutputHelper TestOutputHelper { get; }

    private StepFactoryStore StepFactoryStore { get; }

    [Theory]
    [InlineData("For",          "", "Do an action",         "<i>")]
    [InlineData("StringToCase", "", "../Enums/TextCase.md", "//")]
    [InlineData(
        "RestDelete",
        "",
        "The relative url          |\r\n|Headers",
        null
    )] //make sure the relative url comes before the headers
    [InlineData(
        "Transform",
        "",
        "SCLOneOf<`string`, Array<`string`>, `entity`>",
        "SCLOneOf<`string`,Array<`string`>,`entity`>"
    )]
    public void TestGetStepPage(
        string step,
        string rootUrl,
        string? expectContains,
        string? expectNotContains)
    {
        var stepFactory = StepFactoryStore.Dictionary[step];

        var page = DocumentationCreator.GetStepPage(
            new StepWrapper(stepFactory),
            rootUrl
        );

        TestOutputHelper.WriteLine(page.FileText);

        if (expectContains is not null)
            page.FileText.Should().Contain(expectContains);

        if (expectNotContains is not null)
            page.FileText.Should().NotContain(expectNotContains);
    }
}
