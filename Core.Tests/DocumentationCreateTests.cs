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

        TestText(expectContains, expectNotContains, page.FileText);
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

        TestText(expectContains, expectNotContains, text);
    }

    [Fact]
    public void TestRequirementsStep()
    {
        var stepFactory = new StepRequirementsStep().StepFactory;

        var page = DocumentationCreator.GetStepPage(
            new StepWrapper(stepFactory),
            new DocumentationOptions()
        );

        TestText("*Requires MyConnector.Features: Alpha, Beta*", null, page.FileText);
    }

    private void TestText(string? expectContains, string? expectNotContains, string text)
    {
        TestOutputHelper.WriteLine(text);

        if (expectContains is not null)
            text.Should().Contain(expectContains);

        if (expectNotContains is not null)
            text.Should().NotContain(expectNotContains);
    }

    private class StepRequirementsStep : CompoundStep<SCLInt>
    {
        /// <inheritdoc />
        protected override Task<Result<SCLInt, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return BaseStep.Run(stateMonad, cancellationToken);
        }

        [Required][StepProperty(1)] public IStep<SCLInt> BaseStep { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => StepRequirementsStepFactory.Instance;

        private class StepRequirementsStepFactory : SimpleStepFactory<StepRequirementsStep, SCLInt>

        {
            private StepRequirementsStepFactory() { }
            public static StepRequirementsStepFactory Instance { get; } = new();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements
            {
                get
                {
                    var connectorName = "Reductech.Connectors.MyConnector";

                    yield return new ConnectorRequirement(connectorName);

                    yield return new FeatureRequirement(
                        connectorName,
                        "Features",
                        new[] { "Alpha", "Beta" }
                    );

                    yield return new VersionRequirement(connectorName, "Version");
                }
            }
        }
    }
}
