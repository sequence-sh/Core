using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ReadCSVTests : StepTestBase<ReadCsv, EntityStream>
    {
        /// <inheritdoc />
        public ReadCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new SequenceStepCase("Read CSV and print all lines",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new ForEachEntity
                            {
                                VariableName = new VariableName("Foo"),
                                EntityStream = new ReadCsv
                                {
                                    ColumnsToMap = Constant(new List<string> {"Foo", "Bar"}),
                                    Delimiter = Constant(","),
                                    TextStream = new ToStream
                                    {Text = Constant(
                                        $@"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")}
                                },
                                Action = new Print<Entity>
                                {
                                    Value = new GetVariable<Entity> {VariableName = new VariableName("Foo")}
                                }
                            }
                        }
                    },
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                ).WithExpectedFinalState("Foo", new Entity(new KeyValuePair<string, string>("Foo", "Hello 2"), new KeyValuePair<string, string>("Bar", "World 2")));


            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var (step, _) = CreateStepWithDefaultOrArbitraryValues();

                var expectedYaml = @"Do: ReadCsv
ColumnsToMap:
- 'Foo0'
- 'Foo1'
- 'Foo2'
CommentToken: 'Bar3'
Delimiter: ','
Encoding: EncodingEnum.Default
HasFieldsEnclosedInQuotes: False
TextStream: 'Baz4'";



                yield return new SerializeCase("Default", step, expectedYaml);
            }
        }
    }
}