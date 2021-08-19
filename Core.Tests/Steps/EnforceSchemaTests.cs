using System.Collections.Generic;
using System.Collections.Immutable;
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

    const string SchemaName = "ValueIf Schema";

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
                        Action = new LambdaFunction<Entity, Unit>(
                            null,
                            new Log<Entity> { Value = GetEntityVariable }
                        ),
                        Array =
                            new EnforceSchema
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
                    Entity.Create(("Foo", "Hello"),   ("Bar", "1")),
                    Entity.Create(("Foo", "Hello 2"), ("Bar", "2"))
                },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("foo", SCLType.String, Multiplicity.ExactlyOne),
                    ("Bar", SCLType.Integer, Multiplicity.ExactlyOne)
                ),
                "('Foo': \"Hello\" 'Bar': 1)",
                "('Foo': \"Hello 2\" 'Bar': 2)"
            );

            yield return CreateCase(
                "Cast int",
                new List<Entity> { Entity.Create(("Foo", "100")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Integer, Multiplicity.ExactlyOne)
                ),
                "('Foo': 100)"
            );

            yield return CreateCase(
                "Cast double",
                new List<Entity> { Entity.Create(("Foo", "100.345")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Double, Multiplicity.ExactlyOne)
                ),
                "('Foo': 100.345)"
            );

            yield return CreateCase(
                "Cast bool",
                new List<Entity> { Entity.Create(("Foo", "true")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Bool, Multiplicity.ExactlyOne)
                ),
                "('Foo': True)"
            );

            yield return CreateCase(
                "Cast date time",
                new List<Entity> { Entity.Create(("Foo", "11/10/2020 3:45:44 PM")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Date, Multiplicity.ExactlyOne)
                ),
                "('Foo': 2020-10-11T15:45:44.0000000)"
            );

            yield return CreateCase(
                "Cast date time with input format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Date, null, Multiplicity.ExactlyOne, null, null,
                     new List<string> { "yyyy" }, null)
                ),
                "('Foo': 2020-01-01T00:00:00.0000000)"
            );

            yield return CreateCase(
                "Cast date time with default input format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                new Schema()
                {
                    Name                    = SchemaName,
                    DefaultDateInputFormats = new List<string> { "yyyy" },
                    DefaultErrorBehavior    = ErrorBehavior.Fail,
                    Properties = new Dictionary<string, SchemaProperty>()
                    {
                        {
                            "Foo",
                            new SchemaProperty
                            {
                                Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne
                            }
                        }
                    }.ToImmutableSortedDictionary()
                },
                "('Foo': 2020-01-01T00:00:00.0000000)"
            );

            yield return CreateCase(
                "Cast date time with input format and output format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Date, null, Multiplicity.ExactlyOne, null, null,
                     new List<string> { "yyyy" }, "yyyy-mm-dd")
                ),
                "('Foo': 2020-00-01)"
            );

            yield return CreateCase(
                "Cast date time with default input format and default output format",
                new List<Entity> { Entity.Create(("Foo", "2020")) },
                new Schema()
                {
                    Name                    = SchemaName,
                    DefaultDateInputFormats = new List<string> { "yyyy" },
                    DefaultDateOutputFormat = "yyyy-mm-dd",
                    DefaultErrorBehavior    = ErrorBehavior.Fail,
                    Properties = new Dictionary<string, SchemaProperty>()
                    {
                        {
                            "Foo",
                            new SchemaProperty
                            {
                                Type = SCLType.Date, Multiplicity = Multiplicity.ExactlyOne
                            }
                        }
                    }.ToImmutableSortedDictionary()
                },
                "('Foo': 2020-00-01)"
            );

            yield return CreateCase(
                "Match regex",
                new List<Entity> { Entity.Create(("Foo", "100")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Integer, null, Multiplicity.ExactlyOne, @"\d+", null,
                     null, null)
                ),
                "('Foo': 100)"
            );

            yield return CreateCase(
                "Match enum",
                new List<Entity> { Entity.Create(("Foo", "hello")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Enum, "Word", Multiplicity.ExactlyOne, null,
                     new List<string> { "Hello", "World" }, null, null)
                ),
                "('Foo': Word.hello)"
            );

            yield return CreateCase(
                "Null with multiplicity any",
                new List<Entity> { Entity.Create(("Foo", null)) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Integer, null, Multiplicity.Any, null,
                     null, null, null)
                ),
                "('Foo': \"\")"
            );

            yield return CreateCase(
                "Null with multiplicity UpToOne",
                new List<Entity> { Entity.Create(("Foo", null)) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Integer, null, Multiplicity.UpToOne, null,
                     null, null, null)
                ),
                "('Foo': \"\")"
            );

            yield return CreateCase(
                "Null with multiplicity ExactlyOne",
                new List<Entity> { Entity.Create(("Foo", null)) },
                CreateSchema(
                        SchemaName,
                        ExtraPropertyBehavior.Fail,
                        ("Foo", SCLType.Integer, null, Multiplicity.ExactlyOne, null,
                         null, null, null)
                    ) with
                    {
                        DefaultErrorBehavior = ErrorBehavior.Error
                    },
                "Schema violation: Expected 'Foo' to not be null in ('Foo': \"\")"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Error",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                CreateSchema(
                        SchemaName,
                        ExtraPropertyBehavior.Fail,
                        ("Foo", SCLType.Integer, Multiplicity.Any)
                    )with
                    {
                        DefaultErrorBehavior = ErrorBehavior.Error
                    },
                "Schema violation: 'Hello' is not a Integer in ('Foo': \"Hello\")"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Warning",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                CreateSchema(
                        SchemaName,
                        ExtraPropertyBehavior.Fail,
                        ("Foo", SCLType.Integer, Multiplicity.Any)
                    )with
                    {
                        DefaultErrorBehavior = ErrorBehavior.Warning
                    },
                "Schema violation: 'Hello' is not a Integer in ('Foo': \"Hello\")",
                "('Foo': \"Hello\")"
            );

            yield return CreateCase(
                "Could not cast: Behavior: Skip",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                CreateSchema(
                        SchemaName,
                        ExtraPropertyBehavior.Fail,
                        ("Foo", SCLType.Integer, Multiplicity.Any)
                    )with
                    {
                        DefaultErrorBehavior = ErrorBehavior.Skip
                    }
            );

            yield return CreateCase(
                "Could not cast: Behavior: Ignore",
                new List<Entity> { Entity.Create(("Foo", "Hello")) },
                CreateSchema(
                        SchemaName,
                        ExtraPropertyBehavior.Fail,
                        ("Foo", SCLType.Integer, Multiplicity.Any)
                    )with
                    {
                        DefaultErrorBehavior = ErrorBehavior.Ignore
                    },
                "('Foo': \"Hello\")"
            );

            yield return CreateCase(
                "Extra Property: Allow",
                new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Allow,
                    ("Foo", SCLType.String, Multiplicity.Any)
                ),
                "('Foo': \"Hello\" 'Bar': \"World\")"
            );

            yield return CreateCase(
                "Extra Property: Remove",
                new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Remove,
                    ("Foo", SCLType.String, Multiplicity.Any)
                ),
                "('Foo': \"Hello\")"
            );

            yield return CreateCase(
                "Extra Property: Warning",
                new List<Entity> { Entity.Create(("Foo", "Hello"), ("Bar", "World")) },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Warn,
                    ("Foo", SCLType.String, Multiplicity.Any)
                ),
                "Schema violation: Unexpected Property: 'Bar' in ('Foo': \"Hello\" 'Bar': \"World\")",
                "('Foo': \"Hello\")"
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
                                Action = new LambdaFunction<Entity, Unit>(
                                    null,
                                    new Log<Entity> { Value = GetEntityVariable }
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
            var fooMeat  = Entity.Create(("Foo", "Meat"));
            var barFly   = Entity.Create(("Bar", "Fly"));

            yield return CreateCase(
                "Could not cast",
                new List<Entity> { fooHello },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Integer, Multiplicity.Any)
                ),
                ErrorCode.SchemaViolationWrongType,
                "Hello",
                "Integer",
                fooHello
            );

            yield return CreateCase(
                "Missing enum value",
                new List<Entity> { fooFish },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Enum, "Food", Multiplicity.Any, null,
                     new List<string> { "Meat", "Chips" }, null, null)
                ),
                ErrorCode.SchemaViolationWrongType,
                "Fish",
                "Enum",
                fooFish
            );

            yield return CreateCase(
                "Missing enum name",
                new List<Entity> { fooMeat },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.Enum, null, Multiplicity.Any, null,
                     new List<string> { "Meat", "Chips" }, null, null)
                ),
                ErrorCode.SchemaInvalidMissingEnum
            );

            yield return CreateCase(
                "Regex not matched",
                new List<Entity> { fooFish },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.String, null, Multiplicity.Any, @"\d+", null, null,
                     null)
                ),
                ErrorCode.SchemaViolationUnmatchedRegex,
                "Fish",
                @"\d+",
                fooFish
            );

            yield return CreateCase(
                "Missing property",
                new List<Entity> { fooFish },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.String, Multiplicity.Any),
                    ("Bar", SCLType.String, Multiplicity.AtLeastOne)
                ),
                ErrorCode.SchemaViolationMissingProperty,
                "Bar",
                fooFish
            );

            yield return CreateCase(
                "Extra property",
                new List<Entity> { fooFish, barFly },
                CreateSchema(
                    SchemaName,
                    ExtraPropertyBehavior.Fail,
                    ("Foo", SCLType.String, Multiplicity.Any)
                ),
                ErrorCode.SchemaViolationUnexpectedProperty,
                "Bar",
                barFly
            );
        }
    }

    [Fact]
    public void TestCreateSchema()
    {
        var schema = CreateSchema(
            SchemaName,
            ExtraPropertyBehavior.Fail,
            ("foo", SCLType.String, Multiplicity.ExactlyOne),
            ("Bar", SCLType.Integer, Multiplicity.ExactlyOne)
        );

        schema.Name.Should().Be(SchemaName);

        schema.ExtraProperties.Should().Be(ExtraPropertyBehavior.Fail);

        schema.Properties.Count.Should().Be(2);

        var sc1 = schema.Properties.Values.First();

        sc1.EnumType.Should().BeNull();
        sc1.Values.Should().BeNull();
        sc1.Multiplicity.Should().Be(Multiplicity.ExactlyOne);
        sc1.Regex.Should().BeNull();
        sc1.Type.Should().Be(SCLType.Integer);

        var sc2 = schema.Properties.Values.Skip(1).First();

        sc2.EnumType.Should().BeNull();
        sc2.Values.Should().BeNull();
        sc2.Multiplicity.Should().Be(Multiplicity.ExactlyOne);
        sc2.Regex.Should().BeNull();
        sc2.Type.Should().Be(SCLType.String);
    }

    [Fact]
    public void TestSchemaConvertToEntity()
    {
        var schema = CreateSchema(
            SchemaName,
            ExtraPropertyBehavior.Fail,
            ("foo", SCLType.String, Multiplicity.ExactlyOne),
            ("Bar", SCLType.Integer, Multiplicity.ExactlyOne)
        );

        var entity = schema.ConvertToEntity();

        var name = entity.TryGetValue(nameof(schema.Name)).Map(x => x.GetPrimitiveString());

        name.HasValue.Should().BeTrue();

        name.Value.Should().Be(SchemaName);

        var properties = entity.TryGetValue(nameof(schema.Properties));

        properties.HasValue.Should().BeTrue();

        properties.Value.Should().BeOfType<EntityValue.NestedEntity>();
    }
}

}
