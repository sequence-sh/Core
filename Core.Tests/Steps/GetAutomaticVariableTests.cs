using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class GetAutomaticVariableTests : StepTestBase<GetAutomaticVariable<int>, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic use",
                new Log<int>
                {
                    Value = new ElementAtIndex<int>
                    {
                        Index = Constant(0),
                        Array = new ArrayMap<int>
                        {
                            Array = Array(1, 2, 3),
                            Function = new Sum
                            {
                                Terms = ArrayNew<int>.CreateArray(
                                    new List<IStep<int>>
                                    {
                                        new GetAutomaticVariable<int>(),
                                        Constant(13)
                                    }
                                )
                            }
                        }
                    }
                },
                Unit.Default,
                "14"
            );

            yield return new StepCase(
                "Basic use with different variable name",
                new Log<int>()
                {
                    Value = new ElementAtIndex<int>()
                    {
                        Index = Constant(0),
                        Array = new ArrayMap<int>()
                        {
                            Array = Array(1, 2, 3),
                            Function = new Sum()
                            {
                                Terms = ArrayNew<int>.CreateArray(
                                    new List<IStep<int>>()
                                    {
                                        new GetAutomaticVariable<int>(),
                                        Constant(13)
                                    }
                                )
                            },
                            Variable = new VariableName("MyVar")
                        }
                    }
                },
                Unit.Default,
                "14"
            );

            yield return new StepCase(
                "Nested",
                new Log<int>()
                {
                    Value = new ElementAtIndex<int>()
                    {
                        Index = Constant(0),
                        Array = new ArrayMap<int>()
                        {
                            Array = Array(1, 2, 3),
                            Function = new Sum()
                            {
                                Terms = ArrayNew<int>.CreateArray(
                                    new List<IStep<int>>()
                                    {
                                        new GetAutomaticVariable<int>(),
                                        new ElementAtIndex<int>()
                                        {
                                            Array = new ArrayMap<int>()
                                            {
                                                Array = Array(1, 2, 3),
                                                Function = new Sum()
                                                {
                                                    Terms =
                                                        ArrayNew<
                                                                int>
                                                            .CreateArray(
                                                                new List
                                                                <IStep
                                                                    <int>>()
                                                                {
                                                                    new
                                                                        GetAutomaticVariable
                                                                        <int>(),
                                                                    Constant(12)
                                                                }
                                                            )
                                                },
                                            },
                                            Index = Constant(0)
                                        }
                                    }
                                )
                            }
                        }
                    }
                },
                Unit.Default,
                "14"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase("Basic Case", new GetAutomaticVariable<int>() { }, "<>");
        }
    }
}

}
