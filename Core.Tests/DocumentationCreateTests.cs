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
    [InlineData("For", "", true, "Do an action for each value of &lt;i&gt; in a range", "<i>")]
    public void TestGetStepPage(
        string step,
        string rootUrl,
        bool htmlEncode,
        string? expectContains,
        string? expectNotContains)
    {
        var stepFactory = StepFactoryStore.Dictionary[step];

        var page = DocumentationCreator.GetStepPage(
            new StepWrapper(stepFactory),
            rootUrl,
            htmlEncode
        );

        if (expectContains is not null)
            page.FileText.Should().Contain(expectContains);

        if (expectNotContains is not null)
            page.FileText.Should().NotContain(expectContains);
    }
}
