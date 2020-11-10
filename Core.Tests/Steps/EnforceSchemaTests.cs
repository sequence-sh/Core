using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class EnforceSchemaTests : StepTestBase<EnforceSchema, EntityStream>
    {
        /// <inheritdoc />
        public EnforceSchemaTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                static StepCase CreateCase(string name, EntityStream stream, Schema schema, params string[] expectedLogValues)
                {
                    return new SequenceStepCase(name,

                        new Sequence
                        {
                            Steps = new List<IStep<Unit>>
                            {
                                new ForEachEntity
                                {
                                    VariableName = new VariableName("Foo"),
                                    Action = new Print<Entity>
                                    {
                                        Value = GetVariable<Entity>("Foo")
                                    },
                                    EntityStream =
                                        new EnforceSchema
                                        {
                                            EntityStream = Constant(stream),

                                            Schema = Constant(schema)
                                        }
                                }

                            }
                        }, expectedLogValues
                    ){IgnoreFinalState = true};
                }


                yield return CreateCase("Simple case",
                        EntityStream.Create(CreateEntity(("Foo", "Hello"), ("Bar", "1")), CreateEntity(("Foo", "Hello 2"),("Bar", "2"))),
                        CreateSchema("Test Schema",false, ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne), ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)),
                        "Foo: Hello, Bar: 1","Foo: Hello 2, Bar: 2");


                yield return CreateCase("Cast int",
                    EntityStream.Create(CreateEntity(("Foo", "100"))),
                        CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)),
                        "Foo: 100");


                yield return CreateCase("Cast double",
                    EntityStream.Create(CreateEntity(("Foo", "100.345"))),
                        CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Double, Multiplicity.ExactlyOne)),
                        "Foo: 100.345");

                yield return CreateCase("Cast bool",
                    EntityStream.Create(CreateEntity(("Foo", "true"))),
                        CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Bool, Multiplicity.ExactlyOne)),
                        "Foo: true");

                yield return CreateCase("Cast date time",
                    EntityStream.Create(CreateEntity(("Foo", "11/10/2020 3:45:44 PM"))),
                        CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Date, Multiplicity.ExactlyOne)),
                    "Foo: 11/10/2020 3:45:44 PM");


                yield return CreateCase("Cast multiple values",
                   EntityStream.Create(CreateEntity(("Foo", "10"), ("Foo", "15"))),
                   CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)),
                   "Foo: 10, 15");


                yield return CreateCase("Match regex",
                    EntityStream.Create(CreateEntity(("Foo", "100"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.ExactlyOne, @"\d+", null)),
                        "Foo: 100");

                yield return CreateCase("Match enum",
                    EntityStream.Create(CreateEntity(("Foo", "hello"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Enum, Multiplicity.ExactlyOne, null, new List<string>(){"Hello", "World"})),
                        "Foo: hello");
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                static ErrorCase CreateCase(string name, EntityStream stream, Schema schema, string expectedError, ErrorCode expectedErrorCode)
                {
                    var enforceSchema = new EnforceSchema
                    {
                        EntityStream = Constant(stream),

                        Schema = Constant(schema),
                        ErrorBehaviour = Constant(ErrorBehaviour.Fail)
                    };

                    return new ErrorCase(name,
                        new Sequence
                        {
                            Steps = new List<IStep<Unit>>
                            {
                                new ForEachEntity
                                {
                                    VariableName = new VariableName("Foo"),
                                    Action = new Print<Entity>
                                    {
                                        Value = GetVariable<Entity>("Foo")
                                    },
                                    EntityStream =enforceSchema
                                }

                            }
                        },
                        new ErrorBuilder(expectedError, expectedErrorCode).WithLocation(new StepErrorLocation(enforceSchema))

                    );
                }

                yield return CreateCase("Could not cast",
                    EntityStream.Create(CreateEntity(("Foo", "Hello"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)),
                    "Could not convert 'Hello' to Integer", ErrorCode.SchemaViolation);


                yield return CreateCase("Missing enum",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.Enum, Multiplicity.Any, null, new List<string>(){"Meat", "Chips"})),
                    "Could not convert 'Fish' to Enum", ErrorCode.SchemaViolation);


                yield return CreateCase("Regex not matched",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any, @"\d+", null)),
                    @"'Fish' does not match regex '\d+'", ErrorCode.SchemaViolation);


                yield return CreateCase("Missing property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any), ("Bar", SchemaPropertyType.String, Multiplicity.AtLeastOne)),
                    "Missing property 'Bar'", ErrorCode.SchemaViolation);

                yield return CreateCase("Extra property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"),("Bar", "Fly"))),
                    CreateSchema("Test Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any)),
                    "Unexpected Property 'Bar'", ErrorCode.SchemaViolation);



            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var c = CreateStepWithDefaultOrArbitraryValues();

                yield return new SerializeCase("default",
                    c.step,
                    @"Do: EnforceSchema
EntityStream:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')
Schema:
  Name: Schema6
  Properties:
    MyProp7:
      Type: Integer
      Multiplicity: Any
  AllowExtraProperties: true
ErrorBehaviour: ErrorBehaviour.Fail"
                    );

            }
        }
    }
}