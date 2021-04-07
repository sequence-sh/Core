using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class AssertEqualTests : StepTestBase<AssertEqual<StringStream>, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Strings equal",
                new AssertEqual<StringStream>
                {
                    Left = Constant("Hello"), Right = Constant("Hello")
                },
                Unit.Default
            );

            yield return new StepCase(
                "Ints equal",
                new AssertEqual<int> { Left = Constant(2), Right = Constant(2) },
                Unit.Default
            );

            yield return new StepCase(
                "Entities equal",
                new AssertEqual<Entity>
                {
                    Left  = Constant(Entity.Create(("a", 1))),
                    Right = Constant(Entity.Create(("a", 1)))
                },
                Unit.Default
            );

            yield return new StepCase(
                "Strings not equal",
                new AssertError()
                {
                    Step = new AssertEqual<StringStream>
                    {
                        Left = Constant("Hello"), Right = Constant("World")
                    }
                },
                Unit.Default
            ) { IgnoreLoggedValues = true };

            yield return new StepCase(
                "Ints not equal",
                new AssertError()
                {
                    Step = new AssertEqual<int> { Left = Constant(2), Right = Constant(3) }
                },
                Unit.Default
            ) { IgnoreLoggedValues = true };
        }
    }
}

}
