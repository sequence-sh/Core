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
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EnforceSchemaTests : StepTestBase<EnforceSchema, EntityStream>
    {
        /// <inheritdoc />
        public EnforceSchemaTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-GB");
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                static StepCase CreateCase(string name, EntityStream stream, Schema schema, params string[] expectedLogValues)
                {
                    var schemaEntity = schema.ConvertToEntity();

                    return new StepCase(name,

                        new EntityForEach
                        {
                            Action = new Print<Entity>
                            {
                                Value = GetVariable<Entity>(VariableName.Entity)
                            },
                            EntityStream =
                            new EnforceSchema
                            {
                                EntityStream = Constant(stream),

                                Schema = Constant(schemaEntity)
                            }
                        },
                        Unit.Default
                        , expectedLogValues
                    );
                }


                yield return CreateCase("Simple case",
                        EntityStream.Create(CreateEntity(("Foo", "Hello"), ("Bar", "1")), CreateEntity(("Foo", "Hello 2"), ("Bar", "2"))),
                        CreateSchema("ValueIf Schema", false,
                            ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
                            ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)),
                        "(Foo: \"Hello\" Bar: 1)", "(Foo: \"Hello 2\" Bar: 2)");

                yield return CreateCase("Cast int",
                    EntityStream.Create(CreateEntity(("Foo", "100"))),
                        CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)),
                        "(Foo: 100)");


                yield return CreateCase("Cast double",
                    EntityStream.Create(CreateEntity(("Foo", "100.345"))),
                        CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Double, Multiplicity.ExactlyOne)),
                        "(Foo: 100.345)");

                yield return CreateCase("Cast bool",
                    EntityStream.Create(CreateEntity(("Foo", "true"))),
                        CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Bool, Multiplicity.ExactlyOne)),
                        "(Foo: True)");

                yield return CreateCase("Cast date time",
                    EntityStream.Create(CreateEntity(("Foo", "11/10/2020 3:45:44 PM"))),
                        CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Date, Multiplicity.ExactlyOne)),
                    "(Foo: 2020-10-11T15:45:44.0000000)");



                yield return CreateCase("Match regex",
                    EntityStream.Create(CreateEntity(("Foo", "100"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Integer, null, Multiplicity.ExactlyOne, @"\d+", null)),
                        "(Foo: 100)");

                yield return CreateCase("Match enum",
                    EntityStream.Create(CreateEntity(("Foo", "hello"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Enum, "Word", Multiplicity.ExactlyOne, null, new List<string>() { "Hello", "World" })),
                        "(Foo: Word.hello)");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                static ErrorCase CreateCase(string name, EntityStream stream, Schema schema, string expectedError, ErrorCode expectedErrorCode)
                {
                    var schemaEntity = schema.ConvertToEntity();


                    var enforceSchema = new EnforceSchema
                    {
                        EntityStream = Constant(stream),

                        Schema = Constant(schemaEntity),
                        ErrorBehaviour = Constant(ErrorBehaviour.Fail)
                    };

                    return new ErrorCase(name,
                        new Sequence<Unit>
                        {
                            InitialSteps = new List<IStep<Unit>>
                            {
                                new EntityForEach
                                {
                                    Action = new Print<Entity>
                                    {
                                        Value = GetVariable<Entity>(VariableName.Entity)
                                    },
                                    EntityStream =enforceSchema
                                }

                            },
                            FinalStep = new DoNothing()
                        },
                        new ErrorBuilder(expectedError, expectedErrorCode).WithLocation(new StepErrorLocation(enforceSchema))

                    );
                }

                yield return CreateCase("Could not cast",
                    EntityStream.Create(CreateEntity(("Foo", "Hello"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)),
                    "Could not convert 'Hello' to Integer", ErrorCode.SchemaViolation);


                yield return CreateCase("Missing enum value",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Enum, "Food", Multiplicity.Any, null, new List<string>() { "Meat", "Chips" })),
                    "Could not convert 'Fish' to Enum", ErrorCode.SchemaViolation);

                yield return CreateCase("Missing enum name",
                    EntityStream.Create(CreateEntity(("Foo", "Meat"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Enum, null, Multiplicity.Any, null, new List<string>() { "Meat", "Chips" })),
                    "Schema does not define the name of the enum", ErrorCode.SchemaViolation);

                yield return CreateCase("Regex not matched",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, null, Multiplicity.Any, @"\d+", null)),
                    @"'Fish' does not match regex '\d+'", ErrorCode.SchemaViolation);


                yield return CreateCase("Missing property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any), ("Bar", SchemaPropertyType.String, Multiplicity.AtLeastOne)),
                    "Missing property 'Bar'", ErrorCode.SchemaViolation);

                yield return CreateCase("Extra property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"), ("Bar", "Fly"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any)),
                    "Unexpected Property 'Bar'", ErrorCode.SchemaViolation);
            }
        }

        [Fact]
        public void TestCreateSchema()
        {
            var schema = CreateSchema("ValueIf Schema", false,
                ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
                ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne));

            schema.Name.Should().Be("ValueIf Schema");

            schema.AllowExtraProperties.Should().BeFalse();

            schema.Properties.Count.Should().Be(2);

            var sc1 = schema.Properties.Values.First();

            sc1.EnumType.Should().BeNull();
            sc1.Format.Should().BeNull();
            sc1.Multiplicity.Should().Be(Multiplicity.ExactlyOne);
            sc1.Regex.Should().BeNull();
            sc1.Type.Should().Be(SchemaPropertyType.String);
        }

        [Fact]
        public void TestSchemaConvertToEntity()
        {
            var schema = CreateSchema("ValueIf Schema", false,
                ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne),
                ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne));

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