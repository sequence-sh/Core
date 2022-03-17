using System.Text;
using Reductech.Sequence.ConnectorManagement.Base;
using Reductech.Sequence.Core.Internal.Documentation;
using StepParameter = Reductech.Sequence.Core.Internal.Documentation.StepParameter;

namespace Reductech.Sequence.Core.Tests.Steps;

public partial class DocumentationCreateTests : StepTestBase<DocumentationCreate, Entity>
{
    public const string NotFileText =
        "## Not _Alias_:`Not`\n\n_Output_:`SCLBool`\n\nNegation of a boolean value. \n\n|Parameter|Type |Required|Position|Summary | |:--------|:-------:|:------:|:------:|:-------------------|\n|Boolean |`SCLBool`|✔ |1 |The value to negate.| \n\n|Example SCL|Expected Output| |:----------|:-------------:| |Not true |False | |Not false |True |";

    public const string ExampleFileText =
        "## DocumentationExampleStep _Alias_:`DocumentationExampleStep`\n\n_Output_:`StringStream`\n\n*Requires ValueIf Library.Version 1.2* \n\n|Parameter |Type |Required|Position|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL |Value Delimiter|Summary|\n|:---------------|:-------------:|:------:|:------:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|:------|\n|Alpha<br/>_Alef_|`SCLInt` |✔ |1 |Greater than 1| |1234 |100-300 |201 |Greek 2.1 |Beta |[Alpha](alpha.com)| | |\n|Beta |`string` | |2 | |Two hundred | | | | |Alpha | | | |\n|Gamma |`VariableName` | |3 | | | | | | | | | | |\n|Delta |List<`SCLBool`>| |4 | | | | | | | | |, | |";

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Generate Everything",
                new Log() { Value = new DocumentationCreate() },
                Unit.Default
            ) { IgnoreLoggedValues = true };

            var logDocumentation = new Log { Value = new DocumentationCreate() };

            static MainContents Contents(
                string category,
                bool listCategories,
                params (string name, string category, string comment)[] steps)
            {
                static string GetNameTerm((string name, string category, string _) x) =>
                    $"[{x.name}]({x.category}/{x.name}.md)";

                List<(string header,
                        Func<(string name, string category, string comment), string> getValue)>
                    headerOptions;

                if (listCategories)
                {
                    headerOptions =
                        new List<(string header,
                            Func<(string name, string category, string comment), string> getValue
                            )>()
                        {
                            ("Step", GetNameTerm),
                            ("Connector", x => x.category),
                            ("Summary", x => x.comment)
                        };
                }
                else
                {
                    headerOptions =
                        new List<(string header,
                            Func<(string name, string category, string comment), string> getValue
                            )>() { ("Step", GetNameTerm), ("Summary", x => x.comment) };
                }

                var sb = new StringBuilder();
                sb.AppendLine($"# {category} Steps");
                sb.AppendLine();

                foreach (var (header, getValue) in headerOptions)
                {
                    var maxLength = Math.Max(
                        header.Length,
                        steps.Select(getValue).Max(x => x.Length)
                    );

                    sb.Append($"|{header.PadRight(maxLength)}");
                }

                sb.AppendLine("|");

                foreach (var (header, getValue) in headerOptions)
                {
                    var maxLength = Math.Max(
                        header.Length,
                        steps.Select(getValue).Max(x => x.Length)
                    );

                    sb.Append("|:");
                    sb.Append(new string('-', maxLength - 1));
                }

                sb.AppendLine("|");

                foreach (var step in steps)
                {
                    foreach (var (header, getValue) in headerOptions)
                    {
                        sb.Append('|');

                        var maxLength = Math.Max(
                            header.Length,
                            steps.Select(getValue).Max(x => x.Length)
                        );

                        var term = getValue(step).PadRight(maxLength);

                        sb.Append(term);
                    }

                    sb.AppendLine("|");
                }

                sb.AppendLine();

                var text = sb.ToString().Trim();

                return new MainContents($"all.md", "all", text, "");
            }

            (string nameof, string category, string comment) notHeader = (
                "Not", "Core", "Negation of a boolean value.");

            var notStepPage = new StepPage(
                "Not.md",
                "Not",
                NotFileText,
                "Core",
                "Core",
                "Not",
                new List<string>() { "Not" },
                "Negation of a boolean value.",
                "SCLBool",
                new List<StepParameter>()
                {
                    new(
                        "Boolean",
                        "SCLBool",
                        "The value to negate.",
                        true,
                        new List<string>()
                    )
                }
            );

            var notContents = Contents("Sequence®", true, notHeader);

            var notDocumentationEntity = new DocumentationCreationResult(
                notContents,
                new[]
                {
                    new DocumentationCategory(
                        new CategoryContents(
                            "Core.md",
                            "Core",
                            Contents("Core", false, notHeader).FileText,
                            "",
                            "Core"
                        ),
                        new List<StepPage>() { notStepPage }
                    ),
                },
                System.Array.Empty<EnumPage>()
            );

            yield return new StepCase(
                "Generate Not Documentation",
                logDocumentation,
                Unit.Default,
                notDocumentationEntity.ConvertToEntity().Serialize(SerializeOptions.Serialize)
            ) { TestDeserializeAndRun = false }.WithStepFactoryStore(
                StepFactoryStore.Create(
                    System.Array.Empty<ConnectorData>(),
                    new[] { new SimpleStepFactory<Not, SCLBool>() }
                )
            );

            (string nameof, string category, string comment) exampleStepHeader = (
                "DocumentationExampleStep",
                "Examples",
                "");

            var documentationStepPage = new StepPage(
                "DocumentationExampleStep.md",
                "DocumentationExampleStep",
                ExampleFileText,
                "Examples",
                "Examples",
                "DocumentationExampleStep",
                new[] { "DocumentationExampleStep" },
                "",
                "StringStream",
                new[]
                {
                    new StepParameter(
                        "Alpha",
                        "SCLInt",
                        "",
                        true,
                        new[] { "Alef" }
                    ),
                    new StepParameter(
                        "Beta",
                        "string",
                        "",
                        false,
                        new List<string>()
                    ),
                    new StepParameter(
                        "Gamma",
                        "VariableName",
                        "",
                        false,
                        new List<string>()
                    ),
                    new StepParameter(
                        "Delta",
                        "List<SCLBool>",
                        "",
                        false,
                        new List<string>()
                    ),
                }
            );

            var exampleContents = Contents("Sequence®", true, exampleStepHeader);

            var exampleCreationResult = new DocumentationCreationResult(
                exampleContents,
                new[]
                {
                    new DocumentationCategory(
                        new CategoryContents(
                            "Examples.md",
                            "Examples",
                            Contents("Examples", false, exampleStepHeader).FileText,
                            "",
                            "Examples"
                        ),
                        new List<StepPage>() { documentationStepPage }
                    )
                },
                System.Array.Empty<EnumPage>()
            );

            yield return new StepCase(
                "Example step",
                logDocumentation,
                Unit.Default,
                exampleCreationResult.ConvertToEntity().Serialize(SerializeOptions.Serialize)
            ) { TestDeserializeAndRun = false }.WithStepFactoryStore(
                StepFactoryStore.Create(
                    System.Array.Empty<ConnectorData>(),
                    new[] { DocumentationExampleStep.DocumentationExampleStepFactory.Instance }
                )
            );

            var logFirstTenPageTitles = new ForEach<Entity>()
            {
                Action =
                    new LambdaFunction<Entity, Unit>(
                        null,
                        new Log()
                        {
                            Value =
                                new EntityGetValue<StringStream>()
                                {
                                    Entity   = new GetAutomaticVariable<Entity>(),
                                    Property = new SCLConstant<StringStream>("Title")
                                }
                        }
                    ),
                Array = new ArrayTake<Entity>()
                {
                    Array = new EntityGetValue<Array<Entity>>()
                    {
                        Entity   = new DocumentationCreate(),
                        Property = new SCLConstant<StringStream>("AllPages")
                    },
                    Count = new SCLConstant<SCLInt>(10.ConvertToSCLObject())
                }
            };

            yield return new StepCase(
                "OneOfEnums",
                logFirstTenPageTitles,
                Unit.Default,
                "all",
                "Tests",
                "DocumentationExampleStep2",
                "TextCase"
            ) { TestDeserializeAndRun = false }.WithStepFactoryStore(
                StepFactoryStore.Create(
                    System.Array.Empty<ConnectorData>(),
                    new[] { new DocumentationExampleStep2().StepFactory }
                )
            );

            yield return new StepCase(
                "Test all Step Names",
                logFirstTenPageTitles,
                Unit.Default,
                "all",
                "Core",
                "And",
                "ArrayConcat",
                "ArrayDistinct",
                "ArrayElementAtIndex",
                "ArrayFilter",
                "ArrayFind",
                "ArrayFirst",
                "ArrayGroupBy"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield break;
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield break;
        }
    }

    private class DocumentationExampleStep2 : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        protected override async Task<Result<StringStream, IError>> Run(
            IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            throw new Exception("Cannot run Documentation Example Step");
        }

        /// <summary>
        /// The alpha property. Required.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<SCLOneOf<SCLInt, SCLEnum<TextCase>>> Alpha { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory { get; } =
            new SimpleStepFactory<DocumentationExampleStep2, StringStream>();
    }

    private class DocumentationExampleStep : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        #pragma warning disable 1998
        protected override async Task<Result<StringStream, IError>> Run(
                IStateMonad stateMonad,
                CancellationToken cancellation)
            #pragma warning restore 1998
        {
            throw new Exception("Cannot run Documentation Example Step");
        }

        /// <summary>
        /// The alpha property. Required
        /// </summary>
        [StepProperty(1)]
        [Required]
        [AllowedRange("Greater than 1")]
        [DocumentationURL("alpha.com", "Alpha")]
        [Example("1234")]
        [RecommendedRange("100-300")]
        [RecommendedValue("201")]
        [RequiredVersion("Greek", "2.1")]
        [SeeAlso("Beta")]
        [Alias("Alef")]
        // ReSharper disable UnusedMember.Local
        public IStep<SCLInt> Alpha { get; set; } = null!;

        /// <summary>
        /// The beta property. Not Required.
        /// </summary>
        [StepProperty(2)]
        [SeeAlso("Alpha")]
        [DefaultValueExplanation("Two hundred")]
        public IStep<StringStream> Beta { get; set; } =
            new SCLConstant<StringStream>("Two hundred");

        /// <summary>
        /// The delta property.
        /// </summary>
        [StepListProperty(4)]
        [ValueDelimiter(",")]
        public IReadOnlyList<IStep<SCLBool>> Delta { get; set; } = null!;

        /// <summary>
        /// The Gamma property.
        /// </summary>
        [VariableName(3)]
        public VariableName Gamma { get; set; }
        // ReSharper restore UnusedMember.Local

        /// <inheritdoc />
        public override IStepFactory StepFactory => DocumentationExampleStepFactory.Instance;

        public class
            DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep,
                StringStream>
        {
            private DocumentationExampleStepFactory() { }

            public static SimpleStepFactory<DocumentationExampleStep, StringStream> Instance
            {
                get;
            } =
                new DocumentationExampleStepFactory();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements
            {
                get
                {
                    yield return new VersionRequirement(
                        "ValueIf Library",
                        "Version",
                        new Version(1, 2)
                    );
                }
            }

            /// <inheritdoc />
            public override string Category => "Examples";
        }
    }
}
