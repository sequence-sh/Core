using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        public override Task Test(string key) => base.Test(key);
    }

    public class DocumentationTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                yield return new DocumentationTestCase(
                    new List<IStepFactory>
                    {
                        NotStepFactory.Instance
                    },
                    "# Contents",
                    "|Step       |Summary                     |",
                    "|:---------:|:--------------------------:|",
                    "|[Not](#Not)|Negation of a boolean value.|",
                    "# Core",
                    "<a name=\"Not\"></a>",
                    "## Not",
                    "**Boolean**",
                    "Negation of a boolean value.",
                    "|Parameter|Type  |Required|Summary             |",
                    "|:-------:|:----:|:------:|:------------------:|",
                    "|Boolean  |`bool`|☑️      |The value to negate.|");


                yield return new DocumentationTestCase(
                    new List<IStepFactory>
                    {
                        ArrayStepFactory.Instance
                    },
                    "# Contents",
                    "|Step                 |Summary                                     |",
                    "|:-------------------:|:------------------------------------------:|",
                    "|[Array<T>](#Array<T>)|Represents an ordered collection of objects.|",
                    "# Core",
                    "<a name=\"Array<T>\"></a>",
                    "## Array<T>",
                    "**List<T>**",
                    "Represents an ordered collection of objects.",
                    "|Parameter|Type          |Required|Summary                    |",
                    "|:-------:|:------------:|:------:|:-------------------------:|",
                    "|Elements |IStep<[T](#T)>|☑️      |The elements of this array.|"

                    );


                yield return new DocumentationTestCase(
                    new List<IStepFactory>
                    {
                        ApplyMathOperatorStepFactory.Instance
                    },
                    "# Contents",
                    "|Step                                   |Summary                                                                              |",
                    "|:-------------------------------------:|:-----------------------------------------------------------------------------------:|",
                    "|[ApplyMathOperator](#ApplyMathOperator)|Applies a mathematical operator to two integers. Returns the result of the operation.|",
                    "# Core",
                    "<a name=\"ApplyMathOperator\"></a>",
                    "## ApplyMathOperator",
                    "**Int32**",
                    "Applies a mathematical operator to two integers. Returns the result of the operation.",
                    "|Parameter|Type                         |Required|Summary               |",
                    "|:-------:|:---------------------------:|:------:|:--------------------:|",
                    "|Left     |`int`                        |☑️      |The left operand.     |",
                    "|Operator |[MathOperator](#MathOperator)|☑️      |The operator to apply.|",
                    "|Right    |`int`                        |☑️      |The right operand.    |",
                    "# Enums",
                    "<a name=\"MathOperator\"></a>",
                    "## MathOperator",
                    "An operator that can be applied to two numbers.",
                    "|Name    |Summary                                          |",
                    "|:------:|:-----------------------------------------------:|",
                    "|None    |Sentinel value                                   |",
                    "|Add     |Add the left and right operands.                 |",
                    "|Subtract|Subtract the right operand from the left.        |",
                    "|Multiply|Multiply the left and right operands.            |",
                    "|Divide  |Divide the left operand by the right.            |",
                    "|Modulo  |Reduce the left operand modulo the right.        |",
                    "|Power   |Raise the left operand to the power of the right.|"
                );

                yield return new DocumentationTestCase(
                    new List<IStepFactory>
                    {
                        DocumentationExampleStepFactory.Instance
                    },
                    "# Contents",
                    "|Step                                                 |Summary|",
                    "|:---------------------------------------------------:|:-----:|",
                    "|[DocumentationExampleStep](#DocumentationExampleStep)|       |",
                    "# Examples",
                    "<a name=\"DocumentationExampleStep\"></a>",
                    "## DocumentationExampleStep",
                    "**String**",
                    "*Requires Test Library Version 1.2*",
                    "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                    "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                    "|Alpha    |`int`                        |☑️      |       |Greater than 1|             |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                    "|Beta     |`string`                     |        |       |              |Two hundred  |       |                 |                 |            |Alpha   |                  |               |",
                    "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                    "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |");


                yield return new DocumentationTestCase(
                    new List<IStepFactory>
                    {
                        NotStepFactory.Instance,
                        DocumentationExampleStepFactory.Instance
                    },
                    "# Contents",
                    "|Step                                                 |Summary                     |",
                    "|:---------------------------------------------------:|:--------------------------:|",
                    "|[Not](#Not)                                          |Negation of a boolean value.|",
                    "|[DocumentationExampleStep](#DocumentationExampleStep)|                            |",
                    "# Core",
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
                    "*Requires Test Library Version 1.2*",
                    "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                    "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                    "|Alpha    |`int`                        |☑️      |       |Greater than 1|             |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                    "|Beta     |`string`                     |        |       |              |Two hundred  |       |                 |                 |            |Alpha   |                  |               |",
                    "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                    "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |");

            }
        }

        private class DocumentationTestCase : ITestBaseCaseParallel
        {
            /// <summary>
            /// Create a new DocumentationTestCase
            /// </summary>
            public DocumentationTestCase(
                IReadOnlyCollection<IStepFactory> factories,
                params string[] expectedLines)
            {
                ExpectedLines = expectedLines;
                Factories = factories;
                Name = string.Join(" ", Factories
                    .Select(x=>x.TypeName)
                    .OrderBy(x => x));
            }

            /// <inheritdoc />
            public string Name { get; }

            public IReadOnlyCollection<string> ExpectedLines { get; }

            public IReadOnlyCollection<IStepFactory> Factories { get; }

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                var stepFactoryStore = StepFactoryStore.Create(Factories);
                var step = new GenerateDocumentation();


                var monad = new StateMonad(NullLogger.Instance, EmptySettings.Instance, ExternalProcessRunner.Instance, stepFactoryStore);

                var linesResult = await step.Run(monad, CancellationToken.None);

                linesResult.ShouldBeSuccessful(x=>x.AsString);

                var lines = linesResult.Value;

                foreach (var line in lines) testOutputHelper.WriteLine(line);

                lines.Where(x => !string.IsNullOrWhiteSpace(x)).Should().BeEquivalentTo(ExpectedLines);
            }
        }




        private class DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep, string>
        {
            private DocumentationExampleStepFactory() { }

            public static SimpleStepFactory<DocumentationExampleStep, string> Instance { get; } = new DocumentationExampleStepFactory();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements
            {
                get
                {
                    yield return new Requirement
                    {
                        Name = "Test Library",
                        MinVersion = new Version(1,2)
                    };
                }
            }

            /// <inheritdoc />
            public override string Category => "Examples";
        }

        private class DocumentationExampleStep : CompoundStep<string>
        {
            /// <inheritdoc />
#pragma warning disable 1998
            public override async Task<Result<string, IError>> Run(StateMonad stateMonad, CancellationToken cancellation)
#pragma warning restore 1998
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The alpha property. Required
            /// </summary>
            [StepProperty(Order = 1)]
            [Required]
            [AllowedRange("Greater than 1")]

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
            [DefaultValueExplanation("Two hundred")]
            public IStep<string> Beta { get; set; } = new Constant<string>("Two hundred");


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
