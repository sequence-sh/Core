using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class ArrayFindTests : StepTestBase<ArrayFind<StringStream>, int>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Simple case",
                new ArrayFind<StringStream>()
                {
                    Array = Array(("Hello"), ("World")), Element = Constant("World")
                },
                1
            );

            yield return new StepCase(
                "Duplicate Element",
                new ArrayFind<StringStream>
                {
                    Array = Array(("Hello"), ("World"), ("World")), Element = Constant("World")
                },
                1
            );

            yield return new StepCase(
                "Element not present",
                new ArrayFind<StringStream>
                {
                    Array = Array(("Hello"), ("World"), ("World")), Element = Constant("Mark")
                },
                -1
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Simple Case",
                "ArrayFind Array: ['Hello', 'World'] Element: 'World'",
                1
            );
        }
    }
}
