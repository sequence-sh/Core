using System.Globalization;
using static Sequence.Core.TestHarness.SchemaHelpers;

namespace Sequence.Core.Tests.Steps;

public partial class ValidateTests : StepTestBase<Validate, Array<Entity>>
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
                var schemaEntity = schema.ConvertToEntity();

                return new StepCase(
                    name,
                    new ForEach<Entity>
                    {
                        Action = new LambdaFunction<Entity, Unit>(
                            null,
                            new Log { Value = GetEntityVariable }
                        ),
                        Array =
                            new Validate
                            {
                                EntityStream = Array(entities.ToArray()),
                                Schema       = Constant(schemaEntity)
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
                "Validate date as datetime",
                new List<Entity> { Entity.Create(("MyDate", new DateTime(1990, 1, 6))), },
                new JsonSchemaBuilder()
                    .Title(SchemaName)
                    .AdditionalItems(false)
                    .Properties(
                        ("MyDate",
                         new JsonSchemaBuilder().Type(SchemaValueType.String)
                             .Format(new Format("date-time")))
                    ),
                "('MyDate': 1990-01-06T00:00:00.0000000)"
            );

            yield return CreateCase(
                "Validate string as date",
                new List<Entity> { Entity.Create(("MyDate", "1990-01-06")), },
                new JsonSchemaBuilder()
                    .Title(SchemaName)
                    .AdditionalItems(false)
                    .Properties(
                        ("MyDate",
                         new JsonSchemaBuilder().Type(SchemaValueType.String)
                             .Format(new Format("date")))
                    ),
                "('MyDate': \"1990-01-06\")"
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            static ErrorCase CreateCase(
                string name,
                List<Entity> entities,
                List<string>? logOnInvalidMessages,
                JsonSchema schema,
                ErrorCode expectedErrorCode,
                params object[] expectedErrorArgs)
            {
                var schemaEntity = schema.ConvertToEntity();

                var enforceSchema = new Validate
                {
                    EntityStream  = Array(entities.ToArray()),
                    Schema        = Constant(schemaEntity),
                    ErrorBehavior = Constant(ErrorBehavior.Fail),
                };

                if (logOnInvalidMessages is not null)
                    enforceSchema.OnInvalid = new LambdaFunction<Entity, Unit>(
                        null,
                        new Log()
                        {
                            Value = GetEntityVariable,
                            Severity = new SCLConstant<SCLEnum<Severity>>(
                                new SCLEnum<Severity>(Severity.Error)
                            )
                        }
                    );

                var errorCase = new ErrorCase(
                    name,
                    new Sequence<Unit>
                    {
                        InitialSteps = new List<IStep<Unit>>
                        {
                            new ForEach<Entity>
                            {
                                Action = new LambdaFunction<Entity, Unit>(
                                    null,
                                    new Log { Value = GetEntityVariable }
                                ),
                                Array = enforceSchema,
                            }
                        },
                        FinalStep = new DoNothing()
                    },
                    new ErrorBuilder(expectedErrorCode, expectedErrorArgs).WithLocation(
                        new ErrorLocation(enforceSchema)
                    )
                );

                if (logOnInvalidMessages is not null)
                {
                    errorCase.IgnoreLoggedValues   = false;
                    errorCase.CheckLogLevel        = LogLevel.Warning;
                    errorCase.ExpectedLoggedValues = logOnInvalidMessages;
                }

                return errorCase;
            }

            var fooHello = Entity.Create(("Foo", "Hello"));
            var fooFish  = Entity.Create(("Foo", "Fish"));

            yield return CreateCase(
                "Could not cast",
                new List<Entity> { fooHello },
                null,
                new JsonSchemaBuilder().Title(SchemaName)
                    .Properties(("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))),
                ErrorCode.SchemaViolated,
                "Value is \"string\" but should be \"integer\"",
                "properties.Foo.type",
                0,
                fooHello
            );

            yield return CreateCase(
                "Missing enum value",
                new List<Entity> { fooFish },
                null,
                new JsonSchemaBuilder()
                    .Properties(("Foo", EnumProperty("Apple", "Orange"))),
                ErrorCode.SchemaViolated,
                "Expected value to match one of the values specified by the enum",
                "properties.Foo.enum",
                0,
                fooFish
            );

            yield return CreateCase(
                "Regex not matched",
                new List<Entity> { fooFish },
                null,
                new JsonSchemaBuilder()
                    .Properties(
                        ("Foo",
                         new JsonSchemaBuilder().Type(SchemaValueType.String)
                             .Pattern("apple|orange")
                             .Build())
                    ),
                ErrorCode.SchemaViolated,
                "The string value was not a match for the indicated regular expression",
                "properties.Foo.pattern",
                0,
                fooFish
            );

            yield return CreateCase(
                "Missing property",
                new List<Entity> { fooFish },
                null,
                new JsonSchemaBuilder()
                    .Properties(
                        ("Foo", AnyString),
                        ("Bar", AnyString)
                    )
                    .Required("Foo", "Bar"),
                ErrorCode.SchemaViolated,
                "Required properties [\"Bar\"] were not present",
                "required",
                0,
                fooFish
            );

            yield return CreateCase(
                "Extra property",
                new List<Entity> { fooFish },
                null,
                new JsonSchemaBuilder()
                    .Properties(("Bar", AnyString))
                    .AdditionalProperties(JsonSchema.False),
                ErrorCode.SchemaViolated,
                "All values fail against the false schema",
                "additionalProperties",
                0,
                fooFish
            );

            yield return CreateCase(
                "Required Empty property should not validate",
                new List<Entity>() { Entity.Create(("Foo", "Bar")), Entity.Create(("Foo", "")), },
                null,
                new JsonSchemaBuilder()
                    .Properties(("Foo", AnyString))
                    .Required("Foo"),
                ErrorCode.SchemaViolated,
                "Required properties [\"Foo\"] were not present",
                "required",
                1,
                Entity.Create(("Foo", ""))
            );

            yield return CreateCase(
                "Missing property With OnInvalid",
                new List<Entity> { fooFish },
                new List<string>
                {
                    "('RowNumber': 0 'ErrorMessage': \"Required properties [\\\"Bar\\\"] were not present\" 'Location': \"required\" 'Entity': ('Foo': \"Fish\"))"
                },
                new JsonSchemaBuilder()
                    .Properties(
                        ("Foo", AnyString),
                        ("Bar", AnyString)
                    )
                    .Required("Foo", "Bar"),
                ErrorCode.SchemaViolated,
                "Required properties [\"Bar\"] were not present",
                "required",
                0,
                fooFish
            );
        }
    }

    [Fact]
    public void TestSchemaConvertToEntity()
    {
        var builder = new JsonSchemaBuilder()
                .Title(SchemaName)
            ;

        builder.Properties(
            ("prop1", new JsonSchemaBuilder()
                 .Type(SchemaValueType.String)
                 .MinLength(8)
            ),
            ("prop2", new JsonSchemaBuilder()
                 .Type(SchemaValueType.Number)
                 .MultipleOf(42)
            )
        );

        var schema = builder.Build();

        var entity = schema.ConvertToEntity();

        var name = entity.TryGetValue("Title").Map(x => x.Serialize(SerializeOptions.Primitive));

        name.HasValue.Should().BeTrue();

        name.GetValueOrThrow().Should().Be(SchemaName);

        var properties = entity.TryGetValue("Properties");

        properties.HasValue.Should().BeTrue();

        properties.GetValueOrThrow().Should().BeOfType<Entity>();
    }
}
