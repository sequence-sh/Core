using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ReadCSVTests : StepTestBase<FromCSV, EntityStream>
    {
        /// <inheritdoc />
        public ReadCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Read CSV and print all lines",
                    new EntityForEach
                    {
                        EntityStream = new FromCSV
                        {
                            Stream = Constant(
                                        $@"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity> { Variable = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\" Bar: \"World\")",
                    "(Foo: \"Hello 2\" Bar: \"World 2\")");


                yield return new StepCase("Read CSV and print all lines should ignore missing columns",
                    new EntityForEach
                    {
                        EntityStream = new FromCSV
                        {
                            Stream = Constant(
                                        $@"Foo{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity> { Variable = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\")",
                    "(Foo: \"Hello 2\")");


                yield return new StepCase("Read CSV and print all lines should ignore comments",
                    new EntityForEach
                    {
                        EntityStream = new FromCSV
                        {
                            Stream = Constant(
                                        $@"#this is a comment{Environment.NewLine}Foo{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2")
                        },
                        Action = new Print<Entity>
                        {
                            Value = new GetVariable<Entity> { Variable = VariableName.Entity }
                        }
                    }, Unit.Default,
                    "(Foo: \"Hello\")",
                    "(Foo: \"Hello 2\")");


            }
        }

        ///// <inheritdoc />
        //protected override IEnumerable<DeserializeCase> DeserializeCases {
        //    get
        //    {

        //        var csvString = "Foo\r\nHello,World\r\nHello 2,World 2";


        //        yield return new DeserializeCase(
        //            "Ordered Arguments",

        //            $"EntityForEach (FromCSV (StringToStream \"{csvString}\" EncodingEnum.UTF8) EncodingEnum.UTF8 \",\"  \"#\"  ''''  \"\") (Print (<Entity>))",
        //            Unit.Default,
        //            "(Foo: \"Hello\")",
        //            "(Foo: \"Hello 2\")"


        //            );

        //    } }


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
    }
}