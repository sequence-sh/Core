namespace Reductech.Sequence.Core.Tests.Steps;

public partial class StringReplaceTests : StepTestBase<StringReplace, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple Replace",
                new StringReplace()
                {
                    String     = Constant("Number 1"),
                    Pattern    = Constant(@"\d+"),
                    IgnoreCase = Constant(false),
                    Function = new LambdaFunction<StringStream, StringStream>(
                        VariableName.Item,
                        new StringJoin()
                        {
                            Strings = new ArrayNew<StringStream>
                            {
                                Elements = new List<IStep<StringStream>>
                                {
                                    Constant("_"),
                                    GetVariable<StringStream>(VariableName.Item),
                                    Constant("_")
                                }
                            }
                        }
                    )
                },
                "Number _1_"
            );

            yield return new StepCase(
                "No Replacement",
                new StringReplace()
                {
                    String     = Constant("Number One"),
                    Pattern    = Constant(@"\d+"),
                    IgnoreCase = Constant(false),
                    Function = new LambdaFunction<StringStream, StringStream>(
                        VariableName.Item,
                        new StringJoin()
                        {
                            Strings = new ArrayNew<StringStream>
                            {
                                Elements = new List<IStep<StringStream>>
                                {
                                    Constant("_"),
                                    GetVariable<StringStream>(VariableName.Item),
                                    Constant("_")
                                }
                            }
                        }
                    )
                },
                "Number One"
            );

            yield return new StepCase(
                "Multiple Replacements",
                new StringReplace()
                {
                    String     = Constant("Number 1, 2, 3"),
                    Pattern    = Constant(@"\d+"),
                    IgnoreCase = Constant(false),
                    Function = new LambdaFunction<StringStream, StringStream>(
                        VariableName.Item,
                        new StringJoin()
                        {
                            Strings = new ArrayNew<StringStream>
                            {
                                Elements = new List<IStep<StringStream>>
                                {
                                    Constant("_"),
                                    GetVariable<StringStream>(VariableName.Item),
                                    Constant("_")
                                }
                            }
                        }
                    )
                },
                "Number _1_, _2_, _3_"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            foreach (var errorCase in base.ErrorCases)
            {
                yield return errorCase;
            }

            yield return new ErrorCase(
                "Neither Replace nor Function is set",
                new StringReplace() { String = Constant("blah"), Pattern = Constant("Pattern") },
                ErrorCode.MissingParameter.ToErrorBuilder(nameof(StringReplace.Replace))
            );
        }
    }

    [Fact]
    public void ConflictingParametersShouldNotPassVerify()
    {
        var step = new StringReplace()
        {
            String  = Constant("blah"),
            Pattern = Constant("Pattern"),
            Replace = Constant("Replace"),
            Function = new LambdaFunction<StringStream, StringStream>(
                null,
                Constant("Replacement")
            )
        };

        var stepFactoryStore = StepFactoryStore.Create();

        var verifyResult = step.VerifyThis(stepFactoryStore);

        verifyResult.ShouldBeFailure();

        verifyResult.Error.ToErrorBuilder.Should()
            .Be(
                ErrorCode.ConflictingParameters.ToErrorBuilder(
                    nameof(StringReplace.Replace),
                    nameof(StringReplace.Function)
                )
            );
    }
}
