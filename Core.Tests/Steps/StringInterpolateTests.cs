using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Core.Tests.Steps;

public partial class StringInterpolateTests : StepTestBase<StringInterpolate, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "String Constants",
                new StringInterpolate()
                {
                    Strings = new List<IStep>()
                    {
                        StaticHelpers.Constant("a"),
                        StaticHelpers.Constant("b"),
                        StaticHelpers.Constant("c"),
                    }
                },
                "abc"
            );

            yield return new StepCase(
                "Mixed Constants",
                new StringInterpolate()
                {
                    Strings = new List<IStep>()
                    {
                        StaticHelpers.Constant("abc"), StaticHelpers.Constant(123)
                    }
                },
                "abc123"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            yield return new DeserializeCase("Interpolated string 1", "$\"abc\"",      "abc");
            yield return new DeserializeCase("Interpolated string 2", "$\"abc{123}\"", "abc123");

            yield return new DeserializeCase(
                "Interpolated string 3",
                "$\"abc{123}def\"",
                "abc123def"
            );

            yield return new DeserializeCase(
                "Interpolated string 4",
                "$\"abc{100 + 23}def\"",
                "abc123def"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<SerializeCase> SerializeCases
    {
        get
        {
            yield return new SerializeCase(
                "Single string constant",
                new StringInterpolate()
                {
                    Strings = new List<IStep>() { StaticHelpers.Constant("abc"), }
                },
                "$\"abc\""
            );

            yield return new SerializeCase(
                "Two string constants",
                new StringInterpolate()
                {
                    Strings = new List<IStep>()
                    {
                        StaticHelpers.Constant("abc"), StaticHelpers.Constant("def")
                    }
                },
                "$\"abcdef\""
            );

            yield return new SerializeCase(
                "Two constants",
                new StringInterpolate()
                {
                    Strings = new List<IStep>()
                    {
                        StaticHelpers.Constant("abc"), StaticHelpers.Constant(123)
                    }
                },
                "$\"abc{123}\""
            );
        }
    }
}
