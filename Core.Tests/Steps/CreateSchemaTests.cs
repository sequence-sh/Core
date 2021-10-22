using System.Collections.Generic;
using Json.More;
using Json.Schema;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Core.Tests.SchemaHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class CreateSchemaTests : StepTestBase<CreateSchema, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Create Schema",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities   = Array(Entity.Create(("StringProp1", "abc"), ("IntProp1", 123)))
                },
                Create(
                    new JsonSchemaBuilder()
                        .Title("Test Schema")
                        .Type(SchemaValueType.Object)
                        .AdditionalProperties(JsonSchema.False)
                        .Properties(
                            ("StringProp1", AnyString),
                            ("IntProp1", AnyInt)
                        )
                        .Required("StringProp1", "IntProp1")
                )
            );

            yield return new StepCase(
                "Create Schema from multiple entities",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("IntProp1", 123)),
                        Entity.Create(("StringProp1", "def"), ("intProp1", 456)),
                        Entity.Create(("StringProp1", "def"), ("IntProp2", 123))
                    )
                },
                Create(
                    new JsonSchemaBuilder()
                        .Title("Test Schema")
                        .Type(SchemaValueType.Object)
                        .AdditionalProperties(JsonSchema.False)
                        .Properties(
                            ("StringProp1", AnyString),
                            ("IntProp1", AnyInt),
                            ("IntProp2", AnyInt)
                        )
                        .Required("StringProp1")
                )
            );

            yield return new StepCase(
                "Create Schema from multiple entities with competing properties",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("NumProp1", 123)),
                        Entity.Create(("StringProp1", "def"), ("numProp1", 45.6))
                    )
                },
                Create(
                    new JsonSchemaBuilder()
                        .Title("Test Schema")
                        .Type(SchemaValueType.Object)
                        .AdditionalProperties(JsonSchema.False)
                        .Properties(
                            ("StringProp1", AnyString),
                            ("NumProp1", AnyNumber)
                        )
                        .Required("StringProp1", "NumProp1")
                )
            );

            yield return new StepCase(
                "Create Schema where properties don't appear on all entities",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc")),
                        Entity.Create(("NumProp1", 123))
                    )
                },
                Create(
                    new JsonSchemaBuilder()
                        .Title("Test Schema")
                        .Type(SchemaValueType.Object)
                        .AdditionalProperties(JsonSchema.False)
                        .Properties(
                            ("StringProp1", AnyString),
                            ("NumProp1", AnyInt)
                        )
                )
            );

            yield return new StepCase(
                "Combine String and Int properties",
                new CreateSchema()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("MyProp1", "abc"), ("MyProp2", 456)),
                        Entity.Create(("MyProp1", 123),   ("MyProp2", "def"))
                    )
                },
                Create(
                    new JsonSchemaBuilder()
                        .Title("Test Schema")
                        .Type(SchemaValueType.Object)
                        .AdditionalProperties(JsonSchema.False)
                        .Properties(
                            ("MyProp1", AnyString),
                            ("MyProp2", AnyString)
                        )
                        .Required("MyProp1", "MyProp2")
                )
            );
        }
    }
}

}
