using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class RegexReplaceTests : StepTestBase<RegexReplace, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple Replace",
                new RegexReplace()
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
                new RegexReplace()
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
                new RegexReplace()
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
}

}
