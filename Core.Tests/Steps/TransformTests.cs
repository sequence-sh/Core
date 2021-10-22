using System.Collections.Generic;
using System.Globalization;
using Json.More;
using Json.Schema;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class TransformTests : StepTestBase<Transform, Array<Entity>>
{
    #pragma warning disable CA1822 // Mark members as static
    partial void OnInitialized()
        #pragma warning restore CA1822 // Mark members as static
    {
        CultureInfo.DefaultThreadCurrentCulture   = new CultureInfo("en-GB");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");
    }

    const string SchemaName = "My Schema";

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            static StepCase CreateCase(
                string name,
                List<Entity> entities,
                JsonSchema schema,
                params string[] expectedLogValues)
            {
                var schemaEntity = Entity.Create(schema.ToJsonDocument().RootElement);

                return new StepCase(
                    name,
                    new ForEach<Entity>
                    {
                        Action = new LambdaFunction<Entity, Unit>(
                            null,
                            new Log<Entity> { Value = StaticHelpers.GetEntityVariable }
                        ),
                        Array =
                            new Transform()
                            {
                                EntityStream = StaticHelpers.Array(entities.ToArray()),
                                Schema       = StaticHelpers.Constant(schemaEntity)
                            },
                    },
                    Unit.Default,
                    expectedLogValues
                );
            }

            yield return CreateCase(
                "Simple case",
                new List<Entity>
                {
                    Entity.Create(("Foo", "Hello"),   ("Bar", 1)),
                    Entity.Create(("Foo", "Hello 2"), ("Bar", 2))
                },
                new JsonSchemaBuilder()
                    .Title(SchemaName)
                    .AdditionalItems(false)
                    .Properties(
                        ("foo", new JsonSchemaBuilder().Type(SchemaValueType.String)),
                        ("Bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))
                    ),
                "('Foo': \"Hello\" 'Bar': 1)",
                "('Foo': \"Hello 2\" 'Bar': 2)"
            );

            yield return CreateCase(
                "Transform Integers",
                new List<Entity> { Entity.Create(("Bar", 1)), Entity.Create(("Bar", "2")) },
                new JsonSchemaBuilder()
                    .Title(SchemaName)
                    .AdditionalItems(false)
                    .Properties(("Bar", new JsonSchemaBuilder().Type(SchemaValueType.Integer))),
                "('Bar': 1)",
                "('Bar': 2)"
            );
        }
    }
}

}
