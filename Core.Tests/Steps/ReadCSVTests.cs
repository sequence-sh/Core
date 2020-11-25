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
    public class ReadCSVTests : StepTestBase<ReadCSV, EntityStream>
    {
        /// <inheritdoc />
        public ReadCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Read CSV and print all lines",
                    new ForEachEntity
                    {
                        EntityStream = new ReadCSV
                        {
                            Delimiter = Constant(","),
                            TextStream = new ToStream
                            {
                                Text = Constant(
                                        $@"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")
                            }
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity>() { VariableName = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "Foo: Hello, Bar: World",
                    "Foo: Hello 2, Bar: World 2"
                );


                yield return new StepCase("Read CSV and print all lines should ignore missing columns",
                    new ForEachEntity
                    {
                        EntityStream = new ReadCSV
                        {
                            Delimiter = Constant(","),
                            TextStream = new ToStream
                            {
                                Text = Constant(
                                        $@"Foo{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")
                            }
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity> { VariableName = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "Foo: Hello",
                    "Foo: Hello 2"
                );


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

                const string expectedYaml = @"Do: ReadCSV
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