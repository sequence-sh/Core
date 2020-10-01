using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Documentation;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{
    public class DocumentationTests : DocumentationTestCases
    {
        public DocumentationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(DocumentationTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DocumentationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases
        {
            get
            {
                yield return new DocumentationTestCase(
                    new List<(IStepFactory stepFactory, DocumentationCategory documentationCategory)>
                    {
                        (NotStepFactory.Instance, new DocumentationCategory("Steps"))
                    },
                    "# Contents",
                    "|Step                 |Summary                     |",
                    "|:-------------------:|:--------------------------:|",
                    "|<a name=\"Not\">Not</a>|Negation of a boolean value.|",
                    "# Steps",
                    "<a name=\"Not\"></a>",
                    "## Not",
                    "**Boolean**",
                    "Negation of a boolean value.",
                    "|Parameter|Type  |Required|Summary             |",
                    "|:-------:|:----:|:------:|:------------------:|",
                    "|Boolean  |`bool`|☑️      |The value to negate.|");

                yield return new DocumentationTestCase(
                    new List<(IStepFactory stepFactory, DocumentationCategory documentationCategory)>
                    {
                        (DocumentationExampleStepFactory.Instance, new DocumentationCategory("Examples"))
                    },
                    "# Contents",
                    "|Step                                                           |Summary|",
                    "|:-------------------------------------------------------------:|:-----:|",
                    "|<a name=\"DocumentationExampleStep\">DocumentationExampleStep</a>|       |",
                    "# Examples",
                    "<a name=\"DocumentationExampleStep\"></a>",
                    "## DocumentationExampleStep",
                    "**String**",
                    "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                    "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                    "|Alpha    |`int`                        |☑️      |       |Greater than 1|Two hundred  |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                    "|Beta     |`string`                     |        |       |              |             |       |                 |                 |            |Alpha   |                  |               |",
                    "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                    "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |");


                yield return new DocumentationTestCase(
                    new List<(IStepFactory stepFactory, DocumentationCategory documentationCategory)>
                    {
                        (NotStepFactory.Instance, new DocumentationCategory("Steps")),
                        (DocumentationExampleStepFactory.Instance, new DocumentationCategory("Examples"))
                    },
                    "# Contents",
                    "|Step                                                           |Summary                     |",
                    "|:-------------------------------------------------------------:|:--------------------------:|",
                    "|<a name=\"Not\">Not</a>                                          |Negation of a boolean value.|",
                    "|<a name=\"DocumentationExampleStep\">DocumentationExampleStep</a>|                            |",
                    "# Steps",
                    "<a name=\"Not\"></a>",
                    "## Not",
                    "**Boolean**",
                    "Negation of a boolean value.",
                    "|Parameter|Type  |Required|Summary             |",
                    "|:-------:|:----:|:------:|:------------------:|",
                    "|Boolean  |`bool`|☑️      |The value to negate.|",
                    "# Examples",
                    "<a name=\"DocumentationExampleStep\"></a>",
                    "## DocumentationExampleStep",
                    "**String**",
                    "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                    "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                    "|Alpha    |`int`                        |☑️      |       |Greater than 1|Two hundred  |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                    "|Beta     |`string`                     |        |       |              |             |       |                 |                 |            |Alpha   |                  |               |",
                    "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                    "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |");

            }
        }

        private class DocumentationTestCase : ITestBaseCase
        {
            /// <summary>
            /// Create a new DocumentationTestCase
            /// </summary>
            public DocumentationTestCase(
                IReadOnlyCollection<(IStepFactory stepFactory, DocumentationCategory documentationCategory)> factories,
                params string[] expectedLines)
            {
                ExpectedLines = expectedLines;
                Factories = factories;
                Name = string.Join(" ", Factories
                    .Select(x=>x.stepFactory.TypeName)
                    .OrderBy(x => x));
            }

            /// <inheritdoc />
            public string Name { get; }

            public IReadOnlyCollection<string> ExpectedLines { get; }

            public IReadOnlyCollection<(IStepFactory stepFactory, DocumentationCategory documentationCategory)> Factories { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var documented = Factories
                    .Select(x => new StepWrapper(x.stepFactory,x.documentationCategory));

                var lines = DocumentationCreator.CreateDocumentationLines(documented);

                foreach (var line in lines)
                {
                    testOutputHelper.WriteLine(line);
                }

                lines.Where(x => !string.IsNullOrWhiteSpace(x)).Should().BeEquivalentTo(ExpectedLines);
            }
        }




        private class DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep, string>
        {
            private DocumentationExampleStepFactory() { }

            public static SimpleStepFactory<DocumentationExampleStep, string> Instance { get; } = new DocumentationExampleStepFactory();
        }

        private class DocumentationExampleStep : CompoundStep<string>
        {
            /// <inheritdoc />
            public override Result<string, IRunErrors> Run(StateMonad stateMonad)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The alpha property. Required
            /// </summary>
            [StepProperty(Order = 1)]
            [Required]
            [AllowedRange("Greater than 1")]
            [DefaultValueExplanation("Two hundred")]
            [DocumentationURL("alpha.com")]
            [Example("1234")]
            [RecommendedRange("100-300")]
            [RecommendedValue("201")]
            [RequiredVersion("Greek", "2.1")]
            [SeeAlso("Beta")]
            // ReSharper disable UnusedMember.Local
            public IStep<int> Alpha { get; set; } = null!;


            /// <summary>
            /// The beta property. Not Required.
            /// </summary>
            [StepProperty(Order = 2)]
            [SeeAlso("Alpha")]
            public IStep<string> Beta { get; set; } = null!;


            /// <summary>
            /// The delta property.
            /// </summary>
            [StepListProperty(Order = 4)]
            [ValueDelimiter(",")]
            public IReadOnlyList<IStep<bool>> Delta { get; set; } = null!;

            /// <summary>
            /// The Gamma property.
            /// </summary>
            [VariableName(Order = 3)]
            public VariableName Gamma { get; set; }
            // ReSharper restore UnusedMember.Local

            /// <inheritdoc />
            public override IStepFactory StepFactory => DocumentationExampleStepFactory.Instance;
        }
    }
}
