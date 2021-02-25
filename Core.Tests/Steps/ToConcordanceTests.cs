using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ToConcordanceTests : StepTestBase<ToConcordance, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Write Simple Concordance",
                new Log<StringStream>
                {
                    Value = new ToConcordance()
                    {
                        Entities = Array(
                            Entity.Create(("Foo", "Hello"),   ("Bar", "World")),
                            Entity.Create(("Foo", "Hello 2"), ("Bar", "World 2"))
                        )
                    }
                },
                Unit.Default,
                $"þFooþþBarþ{Environment.NewLine}þHelloþþWorldþ{Environment.NewLine}þHello 2þþWorld 2þ{Environment.NewLine}"
            );

            yield return new StepCase(
                "Write Simple Concordance MultiValue",
                new Log<StringStream>
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
