using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class WriteConcordanceTests : StepTestBase<ToConcordance, StringStream>
{
    /// <inheritdoc />
    public WriteConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Write Simple Concordance",
                new Print<StringStream>
                {
                    Value = new ToConcordance()
                    {
                        Entities = Array(
                            CreateEntity(("Foo", "Hello"),   ("Bar", "World")),
                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))
                        )
                    }
                },
                Unit.Default,
                $"þFooþþBarþ{Environment.NewLine}þHelloþþWorldþ{Environment.NewLine}þHello 2þþWorld 2þ{Environment.NewLine}"
            );

            yield return new StepCase(
                "Write Simple Concordance MultiValue",
                new Print<StringStream>
                {
                    Value = new ToConcordance
                    {
                        Entities = Array(
                            Entity.Create(("Foo", "Hello"), ("Bar", new[] { "World", "Earth" })),
                            Entity.Create(
                                ("Foo", "Hello 2"),
                                ("Bar", new[] { "World 2", "Earth 2" })
                            )
                        )
                    }
                },
                Unit.Default,
                $"þFooþþBarþ{Environment.NewLine}þHelloþþWorld|Earthþ{Environment.NewLine}þHello 2þþWorld 2|Earth 2þ{Environment.NewLine}"
            );
        }
    }
}

}
