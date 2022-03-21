using Reductech.Sequence.ConnectorManagement.Base;

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
}
