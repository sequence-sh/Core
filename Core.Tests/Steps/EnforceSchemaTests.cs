using System.Collections.Generic;
using System.Globalization;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
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

                                            Schema = Constant(schema.ConvertToEntity())
                                        }
                        },
                        Unit.Default
                        , expectedLogValues
                    );
                }


                yield return CreateCase("Simple case",
                        EntityStream.Create(CreateEntity(("Foo", "Hello"), ("Bar", "1")), CreateEntity(("Foo", "Hello 2"),("Bar", "2"))),
                        CreateSchema("ValueIf Schema",false, ("foo", SchemaPropertyType.String, Multiplicity.ExactlyOne), ("Bar", SchemaPropertyType.Integer, Multiplicity.ExactlyOne)),
                        "(Foo: \"Hello\" Bar: 1)","(Foo: \"Hello 2\" Bar: 2)");


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

                yield return CreateCase("Cast multiple values",
                   EntityStream.Create(CreateEntity(("Foo", "10"), ("Foo", "15"))),
                   CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.Any)),
                   "(Foo: [10, 15])");


                yield return CreateCase("Match regex",
                    EntityStream.Create(CreateEntity(("Foo", "100"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Integer, Multiplicity.ExactlyOne, @"\d+", null)),
                        "(Foo: 100)");

                yield return CreateCase("Match enum",
                    EntityStream.Create(CreateEntity(("Foo", "hello"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Enum, Multiplicity.ExactlyOne, null, new List<string>(){"Hello", "World"})),
                        "(Foo: Enum.hello)");
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

                        Schema = Constant(schema.ConvertToEntity()),
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


                yield return CreateCase("Missing enum",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.Enum, Multiplicity.Any, null, new List<string>(){"Meat", "Chips"})),
                    "Could not convert 'Fish' to Enum", ErrorCode.SchemaViolation);


                yield return CreateCase("Regex not matched",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any, @"\d+", null)),
                    @"'Fish' does not match regex '\d+'", ErrorCode.SchemaViolation);


                yield return CreateCase("Missing property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any), ("Bar", SchemaPropertyType.String, Multiplicity.AtLeastOne)),
                    "Missing property 'Bar'", ErrorCode.SchemaViolation);

                yield return CreateCase("Extra property",
                    EntityStream.Create(CreateEntity(("Foo", "Fish"),("Bar", "Fly"))),
                    CreateSchema("ValueIf Schema", false, ("Foo", SchemaPropertyType.String, Multiplicity.Any)),
                    "Unexpected Property 'Bar'", ErrorCode.SchemaViolation);



            }
        }

//        /// <inheritdoc />
//        protected override IEnumerable<SerializeCase> SerializeCases
//        {
//            get
//            {
//                var c = CreateStepWithDefaultOrArbitraryValuesAsync();

//                yield return new SerializeCase("default",
//                    c.step,
//                    @"Do: EnforceSchema
//EntityStream:
//- (Prop1 = 'Val0',Prop2 = 'Val1')
//- (Prop1 = 'Val2',Prop2 = 'Val3')
//- (Prop1 = 'Val4',Prop2 = 'Val5')
//Schema:
//  Name: Schema6
//  Properties:
//    MyProp7:
//      Type: Integer
//      Multiplicity: Any
//  AllowExtraProperties: true
//ErrorBehaviour: ErrorBehaviour.Fail"
//                    );

//            }
//        }
    }
}