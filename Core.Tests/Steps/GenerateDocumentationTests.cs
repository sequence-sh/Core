using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;


namespace Reductech.EDR.Core.Tests.Steps
{
    public class GenerateDocumentationTests : StepTestBase<GenerateDocumentation, List<StringStream>>
    {
        /// <inheritdoc />
        public GenerateDocumentationTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Generate Not Documentation",
                    new GenerateDocumentation(),
                    new List<StringStream>
                    {
                        "# Contents",
                        "|Step       |Summary                     |",
                        "|:---------:|:--------------------------:|",
                        "|[Not](#Not)|Negation of a boolean value.|",
                        "# Core",
                        "<a name=\"Not\"></a>",
                        "## Not",
                        "",
                        "**Boolean**",
                        "",
                        "Negation of a boolean value.",
                        "",
                        "|Parameter|Type  |Required|Summary             |",
                        "|:-------:|:----:|:------:|:------------------:|",
                        "|Boolean  |`bool`|☑️      |The value to negate.|",
                        "",

                    }).WithStepFactoryStore(StepFactoryStore.Create(NotStepFactory.Instance));

                yield return new StepCase("Generate Math Documentation",
                    new GenerateDocumentation(),
                    new List<StringStream>()
                    {
                        "# Contents",
                        "|Step                                   |Summary                                                                              |",
                        "|:-------------------------------------:|:-----------------------------------------------------------------------------------:|",
                        "|[ApplyMathOperator](#ApplyMathOperator)|Applies a mathematical operator to two integers. Returns the result of the operation.|",
                        "# Core",
                        "<a name=\"ApplyMathOperator\"></a>",
                        "## ApplyMathOperator",
                        "",
                        "**Int32**",
                        "",
                        "Applies a mathematical operator to two integers. Returns the result of the operation.",
                        "",
                        "|Parameter|Type                         |Required|Summary               |",
                        "|:-------:|:---------------------------:|:------:|:--------------------:|",
                        "|Left     |`int`                        |☑️      |The left operand.     |",
                        "|Operator |[MathOperator](#MathOperator)|☑️      |The operator to apply.|",
                        "|Right    |`int`                        |☑️      |The right operand.    |",
                        "",
                        "# Enums",
                        "<a name=\"MathOperator\"></a>",
                        "## MathOperator",
                        "An operator that can be applied to two numbers.",
                        "",
                        "|Name    |Summary                                                                                                   |",
                        "|:------:|:--------------------------------------------------------------------------------------------------------:|",
                        "|None    |Sentinel value                                                                                            |",
                        "|Add     |Add the left and right operands.                                                                          |",
                        "|Subtract|Subtract the right operand from the left.                                                                 |",
                        "|Multiply|Multiply the left and right operands.                                                                     |",
                        "|Divide  |Divide the left operand by the right. Attempting to divide by zero will result in an error.               |",
                        "|Modulo  |Reduce the left operand modulo the right.                                                                 |",
                        "|Power   |Raise the left operand to the power of the right. If the right operand is negative, zero will be returned.|",
                        ""
                    }).WithStepFactoryStore(StepFactoryStore.Create(ApplyMathOperatorStepFactory.Instance));


                yield return new StepCase("Generate Array Documentation",
                    new GenerateDocumentation(),
                    new List<StringStream>
                    {
                        "# Contents",
                        "|Step                 |Summary                                     |",
                        "|:-------------------:|:------------------------------------------:|",
                        "|[Array<T>](#Array<T>)|Represents an ordered collection of objects.|",
                        "# Core",
                        "<a name=\"Array<T>\"></a>",
                        "## Array<T>",
                        "",
                        "**List<T>**",
                        "",
                        "Represents an ordered collection of objects.",
                        "",
                        "|Parameter|Type          |Required|Summary                   |",
                        "|:-------:|:------------:|:------:|:------------------------:|",
                        "|Elements |IStep<[T](#T)>|☑️      |The elements of the array.|",
                        "",
                    }
                    ).WithStepFactoryStore(StepFactoryStore.Create(ArrayStepFactory.Instance));

                yield return new StepCase("Example step",
                    new GenerateDocumentation(),
                    new List<StringStream>()
                    {
                        "# Contents",
                        "|Step                                                 |Summary|",
                        "|:---------------------------------------------------:|:-----:|",
                        "|[DocumentationExampleStep](#DocumentationExampleStep)|       |",
                        "# Examples",
                        "<a name=\"DocumentationExampleStep\"></a>",
                        "## DocumentationExampleStep",
                        "",
                        "**String**",
                        "",
                        "*Requires ValueIf Library Version 1.2*",
                        "",
                        "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                        "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                        "|Alpha    |`int`                        |☑️      |       |Greater than 1|             |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                        "|Beta     |`string`                     |        |       |              |Two hundred  |       |                 |                 |            |Alpha   |                  |               |",
                        "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                        "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |",
                        "",
                    }).WithStepFactoryStore(StepFactoryStore.Create(DocumentationExampleStepFactory.Instance));

                yield return new StepCase("Two InitialSteps",
                    new GenerateDocumentation(),
                    new List<StringStream>()
                    {
                        "# Contents",
                        "|Step                                                 |Summary                     |",
                        "|:---------------------------------------------------:|:--------------------------:|",
                        "|[Not](#Not)                                          |Negation of a boolean value.|",
                        "|[DocumentationExampleStep](#DocumentationExampleStep)|                            |",
                        "# Core",
                        "<a name=\"Not\"></a>",
                        "## Not",
                        "",
                        "**Boolean**",
                        "",
                        "Negation of a boolean value.",
                        "",
                        "|Parameter|Type  |Required|Summary             |",
                        "|:-------:|:----:|:------:|:------------------:|",
                        "|Boolean  |`bool`|☑️      |The value to negate.|",
                        "",
                        "# Examples",
                        "<a name=\"DocumentationExampleStep\"></a>",
                        "## DocumentationExampleStep",
                        "",
                        "**String**",
                        "",
                        "*Requires ValueIf Library Version 1.2*",
                        "",
                        "|Parameter|Type                         |Required|Summary|Allowed Range |Default Value|Example|Recommended Range|Recommended Value|Requirements|See Also|URL               |Value Delimiter|",
                        "|:-------:|:---------------------------:|:------:|:-----:|:------------:|:-----------:|:-----:|:---------------:|:---------------:|:----------:|:------:|:----------------:|:-------------:|",
                        "|Alpha    |`int`                        |☑️      |       |Greater than 1|             |1234   |100-300          |201              |Greek 2.1   |Beta    |[Alpha](alpha.com)|               |",
                        "|Beta     |`string`                     |        |       |              |Two hundred  |       |                 |                 |            |Alpha   |                  |               |",
                        "|Gamma    |[VariableName](#VariableName)|        |       |              |             |       |                 |                 |            |        |                  |               |",
                        "|Delta    |IStep<`bool`>                |        |       |              |             |       |                 |                 |            |        |                  |,              |",
                        ""
                    }).WithStepFactoryStore(StepFactoryStore.Create(NotStepFactory.Instance, DocumentationExampleStepFactory.Instance));

            }
        }

        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { yield break; }

        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases { get{ yield break;} }


        private class DocumentationExampleStepFactory : SimpleStepFactory<DocumentationExampleStep, StringStream>
        {
            private DocumentationExampleStepFactory() { }

            public static SimpleStepFactory<DocumentationExampleStep, StringStream> Instance { get; } = new DocumentationExampleStepFactory();

            /// <inheritdoc />
            public override IEnumerable<Requirement> Requirements
            {
                get
                {
                    yield return new Requirement
                    {
                        Name = "ValueIf Library",
                        MinVersion = new Version(1, 2)
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
            public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad,
                CancellationToken cancellation)
#pragma warning restore 1998
            {
                throw new NotImplementedException();
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
            // ReSharper disable UnusedMember.Local
            public IStep<int> Alpha { get; set; } = null!;


            /// <summary>
            /// The beta property. Not Required.
            /// </summary>
            [StepProperty(2)]
            [SeeAlso("Alpha")]
            [DefaultValueExplanation("Two hundred")]
            public IStep<StringStream> Beta { get; set; } = new Constant<StringStream>("Two hundred");


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