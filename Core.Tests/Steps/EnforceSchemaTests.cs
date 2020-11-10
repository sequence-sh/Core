using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
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
                yield return new SequenceStepCase("No Errors",

                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>()
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
                                        EntityStream = Constant(EntityStream.Create(
                                            CreateEntity(new KeyValuePair<string, string>("Foo", "Hello"),
                                                new KeyValuePair<string, string>("Bar", "1")),
                                            CreateEntity(new KeyValuePair<string, string>("Foo", "Hello 2"),
                                                new KeyValuePair<string, string>("Bar", "2")))),

                                        Schema = Constant(new Schema
                                        {
                                            Name = "Test Schema",
                                            AllowExtraProperties = false,
                                            Properties = new Dictionary<string, SchemaProperty>
                                            {
                                                {"Foo", new SchemaProperty{Multiplicity = Multiplicity.ExactlyOne, Type = SchemaPropertyType.String}},
                                                {"Bar", new SchemaProperty{Multiplicity = Multiplicity.ExactlyOne, Type = SchemaPropertyType.Integer}}
                                            }
                                        })
                                    }
                            }

                        }
                    }, "Foo: Hello, Bar: 1",
                    "Foo: Hello 2, Bar: 2"
                ).WithExpectedFinalState("Foo",
                    CreateEntity(new KeyValuePair<string, string>("Foo", "Hello 2"),
                        new KeyValuePair<string, string>("Bar", "2")));
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