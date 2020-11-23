using System;
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
    public class ReadCSVTests : StepTestBase<ReadCsv, EntityStream>
    {
        /// <inheritdoc />
        public ReadCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

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
                ).WithExpectedFinalState("Foo", CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2")));


                yield return new SequenceStepCase("Read CSV and print all lines should ignore missing columns",
                    new Sequence
                    {
                        Steps = new List<IStep<Unit>>
                        {
                            new ForEachEntity
                            {
                                VariableName = new VariableName("Foo"),
                                EntityStream = new ReadCsv
                                {
                                    Delimiter = Constant(","),
                                    TextStream = new ToStream
                                    {Text = Constant(
                                        $@"Foo{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")}
                                },
                                Action = new Print<Entity>
                                {
                                    Value = new GetVariable<Entity> {VariableName = new VariableName("Foo")}
                                }
                            }
                        }
                    },
                    "Foo: Hello",
                    "Foo: Hello 2"
                ).WithExpectedFinalState("Foo", CreateEntity(("Foo", "Hello 2")));


            }
        }


        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                foreach (var errorCase in base.ErrorCases)
                    yield return errorCase;

                //TODO tests for errors if we can find any :)
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var (step, _) = CreateStepWithDefaultOrArbitraryValues();

                const string expectedYaml = @"Do: ReadCsv
CommentToken: 'Bar0'
Delimiter: ','
Encoding: EncodingEnum.Default
IgnoreQuotes: False
TextStream: 'Baz1'";



                yield return new SerializeCase("Default", step, expectedYaml);
            }
        }
    }
}