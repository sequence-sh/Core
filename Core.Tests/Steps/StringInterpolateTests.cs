﻿namespace Sequence.Core.Tests.Steps;

public partial class StringInterpolateTests : StepTestBase<StringInterpolate, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "String Constants",
                new StringInterpolate
                {
                    Strings = new List<IStep> { Constant("a"), Constant("b"), Constant("c"), }
                },
                "abc"
            );

            yield return new StepCase(
                "Mixed Constants",
                new StringInterpolate
                {
                    Strings = new List<IStep> { Constant("abc"), Constant(123) }
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
                new StringInterpolate { Strings = new List<IStep> { Constant("abc"), } },
                "$\"abc\""
            );

            yield return new SerializeCase(
                "Two string constants",
                new StringInterpolate
                {
                    Strings = new List<IStep> { Constant("abc"), Constant("def") }
                },
                "$\"abcdef\""
            );

            yield return new SerializeCase(
                "Two constants",
                new StringInterpolate
                {
                    Strings = new List<IStep> { Constant("abc"), Constant(123) }
                },
                "$\"abc{123}\""
            );
        }
    }
}
