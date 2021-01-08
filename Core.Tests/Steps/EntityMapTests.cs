using System.Collections.Generic;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public class EntityMapTests : StepTestBase<EntityMap, Array<Entity>>
{
    /// <inheritdoc />
    public EntityMapTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Add property",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetEntityVariable },
                    Array = new EntityMap
                    {
                        EntityStream = Array(
                            CreateEntity(("Foo", "Hello")),
                            CreateEntity(("Foo", "Hello 2"))
                        ),
                        Function = new EntitySetValue<StringStream>
                        {
                            Entity   = GetEntityVariable,
                            Property = Constant("Bar"),
                            Value    = Constant("World")
                        }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Hello\" Bar: \"World\")",
                "(Foo: \"Hello 2\" Bar: \"World\")"
            );

            yield return new StepCase(
                "Change property",
                new ForEach<Entity>
                {
                    Action = new Print<Entity> { Value = GetEntityVariable },
                    Array = new EntityMap
                    {
                        EntityStream = Array(
                            CreateEntity(("Foo", "Hello"),   ("Bar", "Earth")),
                            CreateEntity(("Foo", "Hello 2"), ("Bar", "Earth"))
                        ),
                        Function = new EntitySetValue<StringStream>
                        {
                            Entity   = GetEntityVariable,
                            Property = Constant("Bar"),
                            Value    = Constant("World")
                        }
                    },
                    Variable = VariableName.Entity
                },
                Unit.Default,
                "(Foo: \"Hello\" Bar: \"World\")",
                "(Foo: \"Hello 2\" Bar: \"World\")"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            //Do not do default cases as some errors are not propagated due to lazy evaluation

            yield return new ErrorCase(
                "Stream error",
                new EntityMap
                {
                    EntityStream = new FailStep<Array<Entity>> { ErrorMessage = "Stream Fail" },
                    Function     = Constant(Entity.Create(("Key", "Value")))
                },
                new SingleError_Core(EntireSequenceLocation.Instance, ErrorCode_Core.Test, "Stream Fail")
            );
        }
    }
}

}
