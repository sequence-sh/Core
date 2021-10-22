using System.Collections.Generic;
using Json.More;
using Json.Schema;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class CreateSchemaTests : StepTestBase<CreateSchema, Entity>
{
    public static Entity Create(JsonSchema schema) =>
        Entity.Create(schema.ToJsonDocument().RootElement);

    public JsonSchema AnyString => new JsonSchemaBuilder().Type(SchemaValueType.String).Build();
    public JsonSchema AnyInt => new JsonSchemaBuilder().Type(SchemaValueType.Integer).Build();

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
                )

                //new Schema()
                //{
                //    Name = "Test Schema",
                //    Properties =
                //        new Dictionary<string, SchemaProperty>()
                //        {
                //            {
                //                "StringProp1",
                //                new SchemaProperty()
                //                {
                //                    Type         = SCLType.String,
                //                    Multiplicity = Multiplicity.ExactlyOne
                //                }
                //            },
                //            {
                //                "IntProp1",
                //                new SchemaProperty()
                //                {
                //                    Type         = SCLType.Integer,
                //                    Multiplicity = Multiplicity.ExactlyOne
                //                }
                //            },
                //        }.ToImmutableSortedDictionary(),
                //    ExtraProperties = ExtraPropertyBehavior.Fail
                //}.ConvertToEntity()
            );

            //yield return new StepCase(
            //    "Create Schema from multiple entities",
            //    new CreateSchema()
            //    {
            //        SchemaName = Constant("Test Schema"),
            //        Entities = Array(
            //            Entity.Create(("StringProp1", "abc"), ("IntProp1", 123)),
            //            Entity.Create(("StringProp1", "def"), ("intProp1", 456)),
            //            Entity.Create(("StringProp1", "def"), ("IntProp2", 123))
            //        )
            //    },
            //    new Schema()
            //    {
            //        Name = "Test Schema",
            //        Properties =
            //            new Dictionary<string, SchemaProperty>()
            //            {
            //                {
            //                    "StringProp1",
            //                    new SchemaProperty()
            //                    {
            //                        Type         = SCLType.String,
            //                        Multiplicity = Multiplicity.ExactlyOne
            //                    }
            //                },
            //                {
            //                    "IntProp1",
            //                    new SchemaProperty()
            //                    {
            //                        Type = SCLType.Integer, Multiplicity = Multiplicity.UpToOne
            //                    }
            //                },
            //                {
            //                    "IntProp2",
            //                    new SchemaProperty()
            //                    {
            //                        Type = SCLType.Integer, Multiplicity = Multiplicity.UpToOne
            //                    }
            //                },
            //            }.ToImmutableSortedDictionary(),
            //        ExtraProperties = ExtraPropertyBehavior.Fail
            //    }.ConvertToEntity()
            //);

            //yield return new StepCase(
            //    "Create Schema from multiple entities with competing properties",
            //    new CreateSchema()
            //    {
            //        SchemaName = Constant("Test Schema"),
            //        Entities = Array(
            //            Entity.Create(("StringProp1", "abc"), ("NumProp1", 123)),
            //            Entity.Create(("StringProp1", "def"), ("numProp1", 45.6))
            //        )
            //    },
            //    new Schema()
            //    {
            //        Name = "Test Schema",
            //        Properties =
            //            new Dictionary<string, SchemaProperty>()
            //            {
            //                {
            //                    "NumProp1",
            //                    new SchemaProperty()
            //                    {
            //                        Type         = SCLType.Double,
            //                        Multiplicity = Multiplicity.ExactlyOne
            //                    }
            //                },
            //                {
            //                    "StringProp1",
            //                    new SchemaProperty()
            //                    {
            //                        Type         = SCLType.String,
            //                        Multiplicity = Multiplicity.ExactlyOne
            //                    }
            //                },
            //            }.ToImmutableSortedDictionary(),
            //        ExtraProperties = ExtraPropertyBehavior.Fail
            //    }.ConvertToEntity()
            //);
        }
    }
}

}
