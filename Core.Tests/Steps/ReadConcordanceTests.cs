using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class ReadConcordanceTests : StepTestBase<FromConcordance, Array<Entity>>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Read Concordance and print all lines",
                new ForEach<Entity>
                {
                    Array = new FromConcordance()
                    {
                        Stream = Constant(
                            $@"þFooþþBarþ{Environment.NewLine}þHelloþþWorldþ{Environment.NewLine}þHello 2þþWorld 2þ"
                        )
                    },
                    Action = new Print<Entity>
                    {
                        Value = new GetVariable<Entity>() { Variable = VariableName.Entity }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Hello\" Bar: \"World\")",
                "(Foo: \"Hello 2\" Bar: \"World 2\")"
            );

            yield return new StepCase(
                "Read Concordance with multiValue and print all lines",
                new ForEach<Entity>
                {
                    Array = new FromConcordance
                    {
                        Stream = Constant(
                            $@"þFooþþBarþ{Environment.NewLine}þHelloþþWorld|Earthþ{Environment.NewLine}þHello 2þþWorld 2|Earth 2þ"
                        )
                    },
                    Action = new Print<Entity>
                    {
                        Value = new GetVariable<Entity> { Variable = VariableName.Entity }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Hello\" Bar: [\"World\", \"Earth\"])",
                "(Foo: \"Hello 2\" Bar: [\"World 2\", \"Earth 2\"])"
            );
        }
    }
}

}
