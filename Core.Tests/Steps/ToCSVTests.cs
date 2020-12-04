using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class ToCSVTests : StepTestBase<ToCSV, DataStream>
    {
        /// <inheritdoc />
        public ToCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Write Simple CSV",

                    new Print<string>
                    {
                        Value = new StringFromStream
                        {
                            Stream = new ToCSV
                            {
                                Entities = new Constant<EntityStream>(EntityStream.Create(
                                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))

                                        ))
                            }
                        }
                    }, Unit.Default,
                    $"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2{Environment.NewLine}"
                );

                yield return new StepCase("Write Simple CSV with tab delimiter",

                    new Print<string>
                    {
                        Value = new StringFromStream
                        {
                            Stream = new ToCSV
                            {
                                Entities = new Constant<EntityStream>(EntityStream.Create(
                                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))

                                        )),
                                Delimiter = Constant("\t")
                            }
                        }
                    }, Unit.Default,
                    $"Foo\tBar{Environment.NewLine}Hello\tWorld{Environment.NewLine}Hello 2\tWorld 2{Environment.NewLine}"
                );
            }
        }
    }
}