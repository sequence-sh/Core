using System.Collections.Generic;
using Json.Schema;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Core.TestHarness.SchemaHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class SchemaCreateTests : StepTestBase<SchemaCreate, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Create Schema",
                new SchemaCreate
                {
                    SchemaName = Constant("Test Schema"),
                    Entities   = Array(Entity.Create(("StringProp1", "abc"), ("IntProp1", 123)))
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("StringProp1", AnyString),
                        ("IntProp1", AnyInt)
                    )
                    .Required("StringProp1", "IntProp1")
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Create Schema from multiple entities",
                new SchemaCreate
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("IntProp1", 123)),
                        Entity.Create(("StringProp1", "def"), ("intProp1", 456)),
                        Entity.Create(("StringProp1", "def"), ("IntProp2", 123))
                    )
                },
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
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Create Schema from multiple entities with competing properties",
                new SchemaCreate
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("NumProp1", 123)),
                        Entity.Create(("StringProp1", "def"), ("numProp1", 45.6))
                    )
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("StringProp1", AnyString),
                        ("NumProp1", AnyNumber)
                    )
                    .Required("StringProp1", "NumProp1")
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Create Schema where properties don't appear on all entities",
                new SchemaCreate
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc")),
                        Entity.Create(("NumProp1", 123))
                    )
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("StringProp1", AnyString),
                        ("NumProp1", AnyInt)
                    )
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Combine String and Int properties",
                new SchemaCreate
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("MyProp1", "abc"), ("MyProp2", 456)),
                        Entity.Create(("MyProp1", 123),   ("MyProp2", "def"))
                    )
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("MyProp1", AnyString),
                        ("MyProp2", AnyString)
                    )
                    .Required("MyProp1", "MyProp2")
                    .Build()
                    .ConvertToEntity()
            );
        }
    }
}

}
