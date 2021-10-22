using System.Linq;
using System.Text.Json;
using Json.More;
using Json.Schema;

namespace Reductech.EDR.Core.Tests
{

public static class SchemaHelpers
{
    public static Entity Create(JsonSchema schema) =>
        Entity.Create(schema.ToJsonDocument().RootElement);

    public static JsonSchema AnyString =>
        new JsonSchemaBuilder().Type(SchemaValueType.String).Build();

    public static JsonSchema EnumProperty(params object[] values) => new JsonSchemaBuilder()
        .Type(SchemaValueType.String)
        .Enum(
            values.Select(
                x => JsonDocument.Parse(JsonSerializer.Serialize<object>(x)).RootElement.Clone()
            )
        )
        .Build();

    public static JsonSchema AnyInt =>
        new JsonSchemaBuilder().Type(SchemaValueType.Integer).Build();

    public static JsonSchema AnyNumber =>
        new JsonSchemaBuilder().Type(SchemaValueType.Number).Build();

    public static JsonSchema AnyDateTime => new JsonSchemaBuilder().Type(SchemaValueType.String)
        .Format(new Format("date-time"))
        .Build();
}

}
