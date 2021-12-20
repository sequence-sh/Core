using System.Globalization;
using static Reductech.Sequence.Core.TestHarness.SchemaHelpers;

namespace Reductech.Sequence.Core.Tests.Steps;

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

            //yield return CreateCase(
            //    "Cast int",
            //    new List<Entity> { Entity.Create(("Foo", "100")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Integer, Multiplicity.ExactlyOne)
            //    ),
            //    "('Foo': 100)"
            //);

            //yield return CreateCase(
            //    "Cast double",
            //    new List<Entity> { Entity.Create(("Foo", "100.345")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Double, Multiplicity.ExactlyOne)
            //    ),
            //    "('Foo': 100.345)"
            //);

            //yield return CreateCase(
            //    "Cast bool",
            //    new List<Entity> { Entity.Create(("Foo", "true")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Bool, Multiplicity.ExactlyOne)
            //    ),
            //    "('Foo': True)"
            //);

            //yield return CreateCase(
            //    "Cast date time",
            //    new List<Entity> { Entity.Create(("Foo", "11/10/2020 3:45:44 PM")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Date, Multiplicity.ExactlyOne)
            //    ),
            //    "('Foo': 2020-10-11T15:45:44.0000000)"
            //);

            //yield return CreateCase(
            //    "Cast date time with input format",
            //    new List<Entity> { Entity.Create(("Foo", "2020")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Date, null, Multiplicity.ExactlyOne, null, null,
            //         new List<string> { "yyyy" }, null)
            //    ),
            //    "('Foo': 2020-01-01T00:00:00.0000000)"
            //);

            //yield return CreateCase(
            //    "Cast date time with default input format",
            //    new List<Entity> { Entity.Create(("Foo", "2020")) },
            //    new Schema()
            //    {
            //        Name                    = SchemaName,
            //        DefaultDateInputFormats = new List<string> { "yyyy" },
            //        DefaultErrorBehavior    = ErrorBehavior.Fail,
            //        Properties = new Dictionary<string, SchemaProperty>()
            //        {
            //            {
            //                "Foo",
            //                new SchemaProperty
            //                {
            //                    Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne
            //                }
            //            }
            //        }.ToImmutableSortedDictionary()
            //    },
            //    "('Foo': 2020-01-01T00:00:00.0000000)"
            //);

            //yield return CreateCase(
            //    "Cast date time with input format and output format",
            //    new List<Entity> { Entity.Create(("Foo", "2020")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Date, null, Multiplicity.ExactlyOne, null, null,
            //         new List<string> { "yyyy" }, "yyyy-mm-dd")
            //    ),
            //    "('Foo': 2020-00-01)"
            //);

            //yield return CreateCase(
            //    "Cast date time with default input format and default output format",
            //    new List<Entity> { Entity.Create(("Foo", "2020")) },
            //    new Schema()
            //    {
            //        Name                    = SchemaName,
            //        DefaultDateInputFormats = new List<string> { "yyyy" },
            //        DefaultDateOutputFormat = "yyyy-mm-dd",
            //        DefaultErrorBehavior    = ErrorBehavior.Fail,
            //        Properties = new Dictionary<string, SchemaProperty>()
            //        {
            //            {
            //                "Foo",
            //                new SchemaProperty
            //                {
            //                    Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne
            //                }
            //            }
            //        }.ToImmutableSortedDictionary()
            //    },
            //    "('Foo': 2020-00-01)"
            //);

            //yield return CreateCase(
            //    "Match regex",
            //    new List<Entity> { Entity.Create(("Foo", "100")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Integer, null, Multiplicity.ExactlyOne, @"\d+", null,
            //         null, null)
            //    ),
            //    "('Foo': 100)"
            //);

            //yield return CreateCase(
            //    "Match enum",
            //    new List<Entity> { Entity.Create(("Foo", "hello")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Enum, "Word", Multiplicity.ExactlyOne, null,
            //         new List<string> { "Hello", "World" }, null, null)
            //    ),
            //    "('Foo': Word.hello)"
            //);

            //yield return CreateCase(
            //    "Null with multiplicity any",
            //    new List<Entity> { Entity.Create(("Foo", null)) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Integer, null, Multiplicity.Any, null,
            //         null, null, null)
            //    ),
            //    "('Foo': null)"
            //);

            //yield return CreateCase(
            //    "Null with multiplicity UpToOne",
            //    new List<Entity> { Entity.Create(("Foo", null)) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Fail,
            //        ("Foo", SCLType.Integer, null, Multiplicity.UpToOne, null,
            //         null, null, null)
            //    ),
            //    "('Foo': null)"
            //);

            //yield return CreateCase(
            //    "Null with multiplicity ExactlyOne",
            //    new List<Entity> { Entity.Create(("Foo", null)) },
            //    SchemaCreate(
            //            SchemaName,
            //            ExtraPropertyBehavior.Fail,
            //            ("Foo", SCLType.Integer, null, Multiplicity.ExactlyOne, null,
            //             null, null, null)
            //        ) with
            //        {
            //            DefaultErrorBehavior = ErrorBehavior.Error
            //        },
            //    "Schema violation: Expected 'Foo' to not be null in ('Foo': null)"
            //);

            //yield return CreateCase(
            //    "Could not cast: Behavior: Error",
            //    new List<Entity> { Entity.Create(("Foo", "Hello")) },
            //    SchemaCreate(
            //            SchemaName,
            //            ExtraPropertyBehavior.Fail,
            //            ("Foo", SCLType.Integer, Multiplicity.Any)
            //        ) with
            //        {
            //            DefaultErrorBehavior = ErrorBehavior.Error
            //        },
            //    "Schema violation: 'Hello' is not a Integer in ('Foo': \"Hello\")"
            //);

            //yield return CreateCase(
            //    "Could not cast: Behavior: Warning",
            //    new List<Entity> { Entity.Create(("Foo", "Hello")) },
            //    SchemaCreate(
            //            SchemaName,
            //            ExtraPropertyBehavior.Fail,
            //            ("Foo", SCLType.Integer, Multiplicity.Any)
            //        ) with
            //        {
            //            DefaultErrorBehavior = ErrorBehavior.Warning
            //        },
            //    "Schema violation: 'Hello' is not a Integer in ('Foo': \"Hello\")",
            //    "('Foo': \"Hello\")"
            //);

            //yield return CreateCase(
            //    "Could not cast: Behavior: Skip",
            //    new List<Entity> { Entity.Create(("Foo", "Hello")) },
            //    SchemaCreate(
            //            SchemaName,
            //            ExtraPropertyBehavior.Fail,
            //            ("Foo", SCLType.Integer, Multiplicity.Any)
            //        ) with
            //        {
            //            DefaultErrorBehavior = ErrorBehavior.Skip
            //        }
            //);

            //yield return CreateCase(
            //    "Could not cast: Behavior: Ignore",
            //    new List<Entity> { Entity.Create(("Foo", "Hello")) },
            //    SchemaCreate(
            //            SchemaName,
            //            ExtraPropertyBehavior.Fail,
            //            ("Foo", SCLType.Integer, Multiplicity.Any)
            //        ) with
            //        {
            //            DefaultErrorBehavior = ErrorBehavior.Ignore
            //        },
            //    "('Foo': \"Hello\")"
            //);

            //yield return CreateCase(
            //    "Extra Property: Allow",
            //    new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Allow,
            //        ("Foo", SCLType.String, Multiplicity.Any)
            //    ),
            //    "('Foo': \"Hello\" 'Bar': \"World\")"
            //);

            //yield return CreateCase(
            //    "Extra Property: Remove",
            //    new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Remove,
            //        ("Foo", SCLType.String, Multiplicity.Any)
            //    ),
            //    "('Foo': \"Hello\")"
            //);

            //yield return CreateCase(
            //    "Extra Property: Warning",
            //    new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
            //    SchemaCreate(
            //        SchemaName,
            //        ExtraPropertyBehavior.Warn,
            //        ("Foo", SCLType.String, Multiplicity.Any)
            //    ),
            //    "Schema violation: Unexpected Property: 'Bar' in ('Foo': \"Hello\" 'Bar': \"World\")",
            //    "('Foo': \"Hello\")"
            //);
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
                JsonSchema schema,
                ErrorCode expectedErrorCode,
                params object[] expectedErrorArgs)
            {
                var schemaEntity = schema.ConvertToEntity();

                var enforceSchema = new Validate
                {
                    EntityStream  = Array(entities.ToArray()),
                    Schema        = Constant(schemaEntity),
                    ErrorBehavior = Constant(ErrorBehavior.Fail)
                };

                return new ErrorCase(
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
            }

            var fooHello = Entity.Create(("Foo", "Hello"));
            var fooFish  = Entity.Create(("Foo", "Fish"));

            yield return CreateCase(
                "Could not cast",
                new List<Entity> { fooHello },
                new JsonSchemaBuilder().Title(SchemaName)
                    .Properties(("Foo", new JsonSchemaBuilder().Type(SchemaValueType.Integer))),
                ErrorCode.SchemaViolation,
                "Value is string but should be integer",
                "#/properties/Foo/type"
            );

            yield return CreateCase(
                "Missing enum value",
                new List<Entity> { fooFish },
                new JsonSchemaBuilder()
                    .Properties(("Foo", EnumProperty("Apple", "Orange"))),
                ErrorCode.SchemaViolation,
                "Expected value to match one of the values specified by the enum",
                "#/properties/Foo/enum"
            );

            yield return CreateCase(
                "Regex not matched",
                new List<Entity> { fooFish },
                new JsonSchemaBuilder()
                    .Properties(
                        ("Foo",
                         new JsonSchemaBuilder().Type(SchemaValueType.String)
                             .Pattern("apple|orange")
                             .Build())
                    ),
                ErrorCode.SchemaViolation,
                "The string value was not a match for the indicated regular expression",
                "#/properties/Foo/pattern"
            );

            yield return CreateCase(
                "Missing property",
                new List<Entity> { fooFish },
                new JsonSchemaBuilder()
                    .Properties(
                        ("Foo", AnyString),
                        ("Bar", AnyString)
                    )
                    .Required("Foo", "Bar"),
                ErrorCode.SchemaViolation,
                "Required properties [Bar] were not present",
                "#/required"
            );

            yield return CreateCase(
                "Extra property",
                new List<Entity> { fooFish },
                new JsonSchemaBuilder()
                    .Properties(("Bar", AnyString))
                    .AdditionalProperties(JsonSchema.False),
                ErrorCode.SchemaViolation,
                "All values fail against the false schema",
                "#/additionalProperties/$false"
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
