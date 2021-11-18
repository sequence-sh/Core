using System.Collections.Generic;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class
    ArrayElementAtIndexTests : StepTestBase<ArrayElementAtIndex<StringStream>, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Index 0",
                new ArrayElementAtIndex<StringStream>
                {
                    Index = Constant(0), Array = Array(("Hello"), ("World"))
                },
                "Hello"
            );

            yield return new StepCase(
                "Index 1",
                new ArrayElementAtIndex<StringStream>
                {
                    Index = Constant(1), Array = Array(("Hello"), ("World"))
                },
                "World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase(
                "Index 0",
                "ArrayElementAtIndex Array: ['Hello', 'World'] Index: 0",
                "Hello"
            );

            yield return new DeserializeCase(
                "Index 1",
                "ArrayElementAtIndex Array: ['Hello', 'World'] Index: 1",
                "World"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            var (step, _) = CreateStepWithDefaultOrArbitraryValues();

            yield return new SerializeCase("Default", step, "[\"Foo0\", \"Foo1\", \"Foo2\"][3]");
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            yield return new ErrorCase(
                "Index -1",
                new ArrayElementAtIndex<StringStream>
                {
                    Index = Constant(-1), Array = Array(("Hello"), ("World"))
                },
                new ErrorBuilder(ErrorCode.IndexOutOfBounds)
            );

            yield return new ErrorCase(
                "Index too big",
                new ArrayElementAtIndex<StringStream>
                {
                    Index = Constant(2), Array = Array(("Hello"), ("World"))
                },
                new ErrorBuilder(ErrorCode.IndexOutOfBounds)
            );

            foreach (var errorCase in base.ErrorCases)
                yield return errorCase;
        }
    }
}

}
