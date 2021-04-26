using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ArrayEvaluateTests : StepTestBase<ArrayEvaluate<int>, Array<int>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Evaluate eager array",
                new ArrayEvaluate<int>() { Array = Array(1, 2, 3) },
                new EagerArray<int>(new[] { 1, 2, 3 })
            );

            yield return new StepCase(
                "Evaluate lazy array",
                new ArrayEvaluate<int>()
                {
                    Array =
                        new ArrayMap<int>()
                        {
                            Array = Array(1, 2, 3),
                            Function = new Product()
                            {
                                Terms = new ArrayNew<int>()
                                {
                                    Elements = new List<IStep<int>>()
                                    {
                                        Constant(2), GetVariable<int>("number")
                                    }
                                }
                            },
                            Variable = new VariableName("number")
                        }
                },
                new EagerArray<int>(new[] { 2, 4, 6 })
            );

            yield return new StepCase(
                "Read evaluated array twice",
                new Sequence<Unit>()
                {
                    InitialSteps = new List<IStep<Unit>>()
                    {
                        new SetVariable<Array<int>>()
                        {
                            Variable = new VariableName("MyArray"),
                            Value = new ArrayEvaluate<int>()
                            {
                                Array =
                                    new ArrayMap<int>()
                                    {
                                        Array = Array(1, 2, 3),
                                        Function = new Product()
                                        {
                                            Terms = new ArrayNew<int>()
                                            {
                                                Elements = new List<IStep<int>>()
                                                {
                                                    Constant(1),
                                                    GetVariable<int>("number")
                                                }
                                            }
                                        },
                                        Variable = new VariableName("number")
                                    }
                            },
                        },
                        new Log<Array<int>>() { Value = GetVariable<Array<int>>("MyArray") },
                        new Log<Array<int>>() { Value = GetVariable<Array<int>>("MyArray") },
                    },
                    FinalStep = new DoNothing()
                },
                Unit.Default,
                "3 Elements",
                "3 Elements"
            ) { IgnoreFinalState = true };
        }
    }
}

}
