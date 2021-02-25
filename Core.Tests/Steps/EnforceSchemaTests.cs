using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{

public partial class EnforceSchemaTests : StepTestBase<EnforceSchema, Array<Entity>>
{
    #pragma warning disable CA1822 // Mark members as static
    partial void OnInitialized()
        #pragma warning restore CA1822 // Mark members as static
    {
        CultureInfo.DefaultThreadCurrentCulture   = new CultureInfo("en-GB");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            static StepCase CreateCase(
                string name,
                List<Entity> entities,
                Schema schema,
                params string[] expectedLogValues)
            {
                var schemaEntity = schema.ConvertToEntity();

                return new StepCase(
                    name,
                    new ForEach<Entity>
                    {
                        Action = new Log<Entity>
                        {
                            Value = GetVariable<Entity>(VariableName.Entity)
                        },
                        Array =
                            new EnforceSchema
                            {
                                EntityStream = Array(entities.ToArray()),
                                Schema       = Constant(schemaEntity)
                            },
                        Variable = VariableName.Entity
                    },
                    Unit.Default,
                    expectedLogValues
                );
            }

            yield return CreateCase(
                "Simple case",
                new List<Entity>
                {
                    Entity.Create(("Foo", "Hello"),   ("Bar", "1")),
                    Entity.Create(("Foo", "Hello 2"), ("Bar", "2"))
                },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
                    ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)
                ),
                "(Foo: \"Hello\" Bar: 1)",
                "(Foo: \"Hello 2\" Bar: 2)"
            );

            yield return CreateCase(
                "Cast int",
                new List<Entity> { Entity.Create(("Foo", "100")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)
                ),
                "(Foo: 100)"
            );

            yield return CreateCase(
                "Cast double",
                new List<Entity> { Entity.Create(("Foo", "100.345")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Double, Multiplicity.ExactlyOne)
                ),
                "(Foo: 100.345)"
            );

            yield return CreateCase(
                "Cast bool",
                new List<Entity> { Entity.Create(("Foo", "true")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Bool, Multiplicity.ExactlyOne)
                ),
                "(Foo: True)"
            );

            yield return CreateCase(
                "Cast date time",
                new List<Entity> { Entity.Create(("Foo", "11/10/2020 3:45:44 PM")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Date, Multiplicity.ExactlyOne)
                ),
                "(Foo: 2020-10-11T15:45:44.0000000)"
            );

            yield return CreateCase(
                "Cast date time with input format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Date, null, Multiplicity.ExactlyOne, null, null,
                     new List<string> { "yyyy" }, null)
                ),
                "(Foo: 2020-01-01T00:00:00.0000000)"
            );

            yield return CreateCase(
                "Cast date time with input format and output format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Date, null, Multiplicity.ExactlyOne, null, null,
                     new List<string> { "yyyy" }, "yyyy-mm-dd")
                ),
                "(Foo: 2020-00-01)"
            );

            yield return CreateCase(
                "Match regex",
                new List<Entity> { Entity.Create(("Foo", "100")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Integer, null, Multiplicity.ExactlyOne, @"\d+", null,
                     null, null)
                ),
                "(Foo: 100)"
            );

            yield return CreateCase(
                "Match enum",
                new List<Entity> { Entity.Create(("Foo", "hello")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Enum, "Word", Multiplicity.ExactlyOne, null,
                     new List<string> { "Hello", "World" }, null, null)
                ),
                "(Foo: Word.hello)"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Error",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                WithErrorBehavior(
                    CreateSchema(
                        "ValueIf Schema",
                        false,
                        ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)
                    ),
                    ErrorBehavior.Error
                ),
                "Schema violation: Schema Violated: 'Hello' is not a Integer"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Warning",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                WithErrorBehavior(
                    CreateSchema(
                        "ValueIf Schema",
                        false,
                        ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)
                    ),
                    ErrorBehavior.Warning
                ),
                "Schema violation: Schema Violated: 'Hello' is not a Integer",
                "(Foo: \"Hello\")"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Skip",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                WithErrorBehavior(
                    CreateSchema(
                        "ValueIf Schema",
                        false,
                        ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)
                    ),
                    ErrorBehavior.Skip
                )
            );

            yield return CreateCase(
                "Could not cast: Behavior: Ignore",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                WithErrorBehavior(
                    CreateSchema(
                        "ValueIf Schema",
                        false,
                        ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)
                    ),
                    ErrorBehavior.Ignore
                ),
                "(Foo: \"Hello\")"
            );
        }
    }

    private static Schema WithErrorBehavior(Schema schema, ErrorBehavior eb)
    {
        schema.DefaultErrorBehavior = eb;
        return schema;
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            static ErrorCase CreateCase(
                string name,
                List<Entity> entities,
                Schema schema,
                ErrorCode expectedErrorCode,
                params object[] expectedErrorArgs)
            {
                var schemaEntity = schema.ConvertToEntity();

                var enforceSchema = new EnforceSchema
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
                                Action = new Log<Entity>
                                {
                                    Value = GetVariable<Entity>(VariableName.Entity)
                                },
                                Array    = enforceSchema,
                                Variable = VariableName.Entity
                            }
                        },
                        FinalStep = new DoNothing()
                    },
                    new ErrorBuilder(expectedErrorCode, expectedErrorArgs).WithLocation(
                        new ErrorLocation(enforceSchema)
                    )
                );
            }

            yield return CreateCase(
                "Could not cast",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)
                ),
                ErrorCode.SchemaViolationWrongType,
                "Hello",
                "Integer"
            );

            yield return CreateCase(
                "Missing enum value",
                new List<Entity> { Entity.Create(("Foo", "Fish")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Enum, "Food", Multiplicity.Any, null,
                     new List<string> { "Meat", "Chips" }, null, null)
                ),
                ErrorCode.SchemaViolationWrongType,
                "Fish",
                "Enum"
            );

            yield return CreateCase(
                "Missing enum name",
                new List<Entity> { Entity.Create(("Foo", "Meat")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.Enum, null, Multiplicity.Any, null,
                     new List<string> { "Meat", "Chips" }, null, null)
                ),
                ErrorCode.SchemaInvalidMissingEnum
            );

            yield return CreateCase(
                "Regex not matched",
                new List<Entity> { Entity.Create(("Foo", "Fish")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.String, null, Multiplicity.Any, @"\d+", null, null,
                     null)
                ),
                ErrorCode.SchemaViolationUnmatchedRegex,
                "Fish",
                @"\d+"
            );

            yield return CreateCase(
                "Missing property",
                new List<Entity> { Entity.Create(("Foo", "Fish")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.String, Multiplicity.Any),
                    ("Bar", SchemaPropertyType.String, Multiplicity.AtLeastOne)
                ),
                ErrorCode.SchemaViolationMissingProperty,
                "Bar"
            );

            yield return CreateCase(
                "Extra property",
                new List<Entity> { Entity.Create(("Foo", "Fish"), ("Bar", "Fly")) },
                CreateSchema(
                    "ValueIf Schema",
                    false,
                    ("Foo", SchemaPropertyType.String, Multiplicity.Any)
                ),
                ErrorCode.SchemaViolationUnexpectedProperty,
                "Bar"
            );
        }
    }

    [Fact]
    public void TestCreateSchema()
    {
        var schema = CreateSchema(
            "ValueIf Schema",
            false,
            ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
            ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)
        );

        schema.Name.Should().Be("ValueIf Schema");

        schema.AllowExtraProperties.Should().BeFalse();

        schema.Properties.Count.Should().Be(2);

        var sc1 = schema.Properties.Values.First();

        sc1.EnumType.Should().BeNull();
        sc1.Values.Should().BeNull();
        sc1.Multiplicity.Should().Be(Multiplicity.ExactlyOne);
        sc1.Regex.Should().BeNull();
        sc1.Type.Should().Be(SchemaPropertyType.String);
    }

    [Fact]
    public void TestSchemaConvertToEntity()
    {
        var schema = CreateSchema(
            "ValueIf Schema",
            false,
            ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
            ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)
        );

        var entity = schema.ConvertToEntity();

        var name = entity.TryGetValue(nameof(schema.Name)).Map(x => x.GetString());

        name.HasValue.Should().BeTrue();

        name.Value.Should().Be("ValueIf Schema");

        var properties = entity.TryGetValue(nameof(schema.Properties));

        properties.HasValue.Should().BeTrue();

        var propertiesEntity = properties.Value.TryGetEntity();

        propertiesEntity.HasValue.Should().BeTrue();
    }
}

}
