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
    [InlineData("For",          "Do an action",         "<i>")]
    [InlineData("StringToCase", "../Enums/TextCase.md", "//")]
    [InlineData(
        "StringToCase",
        "](https://sequence.sh/steps/Enums/TextCase.md)",
        null,
        "https://sequence.sh/steps"
    )]
    [InlineData(
        "StringToCase",
        "](https://sequence.sh/steps/Enums/TextCase)",
        null,
        "https://sequence.sh/steps",
        false
    )]
    [InlineData(
        "Transform",
        "`string` or `array<string>` or `entity`",
        null
    )]
    [InlineData("ArraySort", "```scl", null)]
    [InlineData("Not",       "```scl", null, "https://sequence.sh/steps", false, true)]
    public void TestGetStepPage(
        string step,
        string? expectContains,
        string? expectNotContains,
        string rootUrl = "",
        bool includeExtensions = true,
        bool includeLink = false,
        bool includeExamples = true)
    {
        var stepFactory = StepFactoryStore.Dictionary[step];

        var page = DocumentationCreator.GetStepPage(
            new StepWrapper(stepFactory),
            new DocumentationOptions(rootUrl, includeExtensions, includeLink, includeExamples)
        );

        TestOutputHelper.WriteLine(page.FileText);

        if (expectContains is not null)
            page.FileText.Should().Contain(expectContains);

        if (expectNotContains is not null)
            page.FileText.Should().NotContain(expectNotContains);
    }

    [Theory]
    [InlineData("For", "Do an action", "<i>")]
    public void TestContents(
        string step,
        string? expectContains,
        string? expectNotContains)
    {
        var stepFactory = StepFactoryStore.Dictionary[step];

        var createResult = DocumentationCreator.CreateDocumentation(
            new List<IDocumentedStep>() { new StepWrapper(stepFactory) },
            new DocumentationOptions()
        );

        var text = createResult.MainContents.FileText;

        TestOutputHelper.WriteLine(text);

        if (expectContains is not null)
            text.Should().Contain(expectContains);

        if (expectNotContains is not null)
            text.Should().NotContain(expectNotContains);
    }
}
