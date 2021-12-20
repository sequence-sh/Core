using static Reductech.Sequence.Core.TestHarness.SchemaHelpers;

namespace Reductech.Sequence.Core.Tests.Steps;

public partial class SchemaCreateCoercedTests : StepTestBase<SchemaCreateCoerced, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new StepCase(
                "Basic Create Schema",
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities   = Array(Entity.Create(("StringProp1", "abc"), ("IntProp1", "123")))
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
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("IntProp1", "123")),
                        Entity.Create(("StringProp1", "def"), ("intProp1", "456")),
                        Entity.Create(("StringProp1", "def"), ("IntProp2", "789"))
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
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc"), ("NumProp1", "123")),
                        Entity.Create(("StringProp1", "def"), ("numProp1", "45.6"))
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
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(("StringProp1", "abc")),
                        Entity.Create(("NumProp1", "123"))
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
                "Create Schema with specified type conversions",
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(
                            ("BoolProp", "untrue"),
                            ("NullProp", "nothing"),
                            ("DateProp", "1990-01-06")
                        )
                    ),
                    BooleanFalseFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant("untrue")
                        ),
                    NullFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant("nothing")
                        ),
                    DateInputFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant("yyyy-MM-dd")
                        )
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("BoolProp", AnyBool),
                        ("NullProp", SchemaNull),
                        ("DateProp", AnyDateTime)
                    )
                    .Required("BoolProp", "NullProp", "DateProp")
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Create Schema with date",
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities   = Array(Entity.Create(("DateProp", "2018-11-13T20:20:39+00:00"))),
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(("DateProp", AnyDateTime))
                    .Required("DateProp")
                    .Build()
                    .ConvertToEntity()
            );

            yield return new StepCase(
                "Create Schema with path dependent specified type conversions",
                new SchemaCreateCoerced()
                {
                    SchemaName = Constant("Test Schema"),
                    Entities = Array(
                        Entity.Create(
                            ("BoolProp", "untrue"),
                            ("NullProp", "nothing"),
                            ("DateProp1", "1990-01-06"),
                            ("DateProp2", "06-01-1990")
                        )
                    ),
                    BooleanFalseFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant("untrue")
                        ),
                    NullFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant("nothing")
                        ),
                    DateInputFormats =
                        new OneOfStep<StringStream, Array<StringStream>, Entity>(
                            Constant(
                                Entity.Create(
                                    ("DateProp1", "yyyy-MM-dd"),
                                    ("DateProp2", "dd-MM-yyyy")
                                )
                            )
                        )
                },
                new JsonSchemaBuilder()
                    .Title("Test Schema")
                    .Type(SchemaValueType.Object)
                    .AdditionalProperties(JsonSchema.False)
                    .Properties(
                        ("BoolProp", AnyBool),
                        ("NullProp", SchemaNull),
                        ("DateProp1", AnyDateTime),
                        ("DateProp2", AnyDateTime)
                    )
                    .Required("BoolProp", "NullProp", "DateProp1", "DateProp2")
                    .Build()
                    .ConvertToEntity()
            );
        }
    }
}
