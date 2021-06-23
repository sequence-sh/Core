using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GenerateDocumentationTests : StepTestBase<GenerateDocumentation, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            var logDocumentation = new Log<Entity> { Value = new GenerateDocumentation() };

            static MainContents Contents(
                params (string name, string category, string comment)[] steps)
            {
                static string GetNameTerm(string n, string category) => $"[{n}]({category}/{n}.md)";

                var maxNameLength = Math.Max(
                    4,
                    steps.Max(x => GetNameTerm(x.name, x.category).Length)
                );

                var maxCommentLength = Math.Max(7, steps.Max(x => x.comment.Length));

                var nameSpaces    = string.Join("", Enumerable.Repeat(' ', maxNameLength - 4));
                var commentSpaces = string.Join("", Enumerable.Repeat(' ', maxCommentLength - 7));

                var nameDashes    = string.Join("", Enumerable.Repeat('-', maxNameLength - 1));
                var commentDashes = string.Join("", Enumerable.Repeat('-', maxCommentLength - 1));

                var sb = new StringBuilder();
                sb.AppendLine("# Contents");
                sb.AppendLine();
                sb.AppendLine($"|Step{nameSpaces}|Summary{commentSpaces}|");
                sb.AppendLine($"|:{nameDashes}|:{commentDashes}|");

                foreach (var (name, category, comment) in steps)
                {
                    sb.AppendLine(
                        $"|{GetNameTerm(name, category).PadRight(maxNameLength)}|{comment.PadRight(maxCommentLength)}|"
                    );
                }

                sb.AppendLine();

                var text = sb.ToString().Trim();

                return new MainContents("Contents.md", "Contents", text, " ");
            }

            (string nameof, string category, string comment) notHeader = (
                "Not", "Core", "Negation of a boolean value.");

            var notStepPage = new StepPage(
                "Not.md",
                "Not",
                "## Not\n_Alias_:`Not`\n\n_Output_:`Boolean`\n\nNegation of a boolean value.\n\n\n|Parameter|Type |Required|Summary |\n|:--------|:----:|:------:|:-------------------|\n|Boolean |`bool`|✔ |The value to negate.|",
                "Core",
                "Core",
                "Not",
                new List<string>() { "Not" },
                "Negation of a boolean value.",
                "Boolean",
                new List<StepParameter>()
                {
                    new(
                        "Boolean",
                        "bool",
                        "The value to negate.",
                        true,
                        new List<string>()
                    )
                }
            );

            var notContents = Contents(notHeader);

            var notDocumentationEntity = new DocumentationCreationResult(
                notContents,
                new[]
                {
                    new DocumentationCategory(
                        new CategoryContents(
                            notContents.FileName,
                            notContents.Title,
                            notContents.FileText,
                            "Core",
                            "Core"
                        ),
                        new List<StepPage>() { notStepPage }
                    ),
                },
                Array.Empty<EnumPage>()
            );

            yield return new StepCase(
                "Generate Not Documentation",
                logDocumentation,
                Unit.Default,
                notDocumentationEntity.ConvertToEntity().Serialize()
            ) { TestDeserializeAndRun = false }.WithStepFactoryStore(
                StepFactoryStore.Create(
                    Array.Empty<ConnectorData>(),
                    new SimpleStepFactory<Not, bool>()
                )
            );

            (string nameof, string category, string comment) exampleStepHeader = (
                "DocumentationExampleStep",
                "Examples",
                "");

            var exampleContents = Contents(exampleStepHeader);

            var documentationStepPage = new StepPage(
                "DocumentationExampleStep.md",
                "DocumentationExampleStep",
                "## DocumentationExampleStep\n_Alias_:`DocumentationExampleStep`\n\n_Output_:`StringStream`\n\n*Requires ValueIf Library Version 1.2*\n\n\n|Parameter |Type |Required|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL |Value Delimiter|Summary|\n|:--------------|:------------:|:------:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|:------|\n|Alpha<br>_Alef_|`int` |✔ |Greater than 1| |1234 |100-300 |201 |Greek 2.1 |Beta |[Alpha](alpha.com)| | |\n|Beta |`string` | | |Two hundred | | | | |Alpha | | | |\n|Gamma |`VariableName`| | | | | | | | | | | |\n|Delta |List<`bool`> | | | | | | | | | |, | |",
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
                        "int",
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
                        "List<bool>",
                        "",
                        false,
                        new List<string>()
                    ),
                }
            );

            var exampleCreationResult = new DocumentationCreationResult(
                exampleContents,
                new[]
                {
                    new DocumentationCategory(
                        new CategoryContents(
                            exampleContents.FileName,
                            exampleContents.Title,
                            exampleContents.FileText,
                            "Examples",
                            "Examples"
                        ),
                        new List<StepPage>() { documentationStepPage }
                    )
                },
                Array.Empty<EnumPage>()
            );

            yield return new StepCase(
                "Example step",
                logDocumentation,
                Unit.Default,
                exampleCreationResult.ConvertToEntity().Serialize()
            ) { TestDeserializeAndRun = false }.WithStepFactoryStore(
                StepFactoryStore.Create(
                    System.Array.Empty<ConnectorData>(),
                    DocumentationExampleStepFactory.Instance
                )
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get { yield break; }
    }

    private class
        DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep, StringStream>
    {
        private DocumentationExampleStepFactory() { }

        public static SimpleStepFactory<DocumentationExampleStep, StringStream> Instance { get; } =
            new DocumentationExampleStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Requirement> Requirements
        {
            get
            {
                yield return new Requirement
                {
                    Name = "ValueIf Library", MinVersion = new Version(1, 2)
                };
            }
        }

        /// <inheritdoc />
        public override string Category => "Examples";
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
        [DocumentationURL("alpha.com")]
        [Example("1234")]
        [RecommendedRange("100-300")]
        [RecommendedValue("201")]
        [RequiredVersion("Greek", "2.1")]
        [SeeAlso("Beta")]
        [Alias("Alef")]
        // ReSharper disable UnusedMember.Local
        public IStep<int> Alpha { get; set; } = null!;

        /// <summary>
        /// The beta property. Not Required.
        /// </summary>
        [StepProperty(2)]
        [SeeAlso("Alpha")]
        [DefaultValueExplanation("Two hundred")]
        public IStep<StringStream> Beta { get; set; } = new StringConstant("Two hundred");

        /// <summary>
        /// The delta property.
        /// </summary>
        [StepListProperty(4)]
        [ValueDelimiter(",")]
        public IReadOnlyList<IStep<bool>> Delta { get; set; } = null!;

        /// <summary>
        /// The Gamma property.
        /// </summary>
        [VariableName(3)]
        public VariableName Gamma { get; set; }
        // ReSharper restore UnusedMember.Local

        /// <inheritdoc />
        public override IStepFactory StepFactory => DocumentationExampleStepFactory.Instance;
    }
}

}
