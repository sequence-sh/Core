using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EntityCombineTests : StepTestBase<EntityCombine, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Combine two simple entities",
                new EntityCombine
                {
                    First  = Constant(CreateEntity(("Foo", 1))),
                    Second = Constant(CreateEntity(("Bar", 2))),
                },
                CreateEntity(("Foo", 1), ("Bar", 2))
            );

            yield return new StepCase(
                "Combine two simple entities with property override",
                new EntityCombine
                {
                    First  = Constant(CreateEntity(("Foo", 1), ("Bar", 1))),
                    Second = Constant(CreateEntity(("Bar", 2))),
                },
                CreateEntity(("Foo", 1), ("Bar", 2))
            );

            yield return new StepCase(
                "Combine nested entities",
                new EntityCombine
                {
                    First  = Constant(CreateEntity(("Foo", CreateEntity(("Bar", 2))))),
                    Second = Constant(CreateEntity(("Foo", CreateEntity(("Baz", 3))))),
                },
                CreateEntity(("Foo", CreateEntity(("Bar", 2), ("Baz", 3))))
            );
        }
    }
}

}
