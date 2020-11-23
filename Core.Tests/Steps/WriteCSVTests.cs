using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class WriteCSVTests : StepTestBase<WriteCSV, Stream>
    {
        /// <inheritdoc />
        public WriteCSVTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return  new SequenceStepCase("Write Simple CSV",

                    new Sequence
                    {
                        Steps = new[]
                        {
                            new Print<string>
                            {
                                Value = new FromStream
                                {
                                    Stream = new WriteCSV
                                    {
                                        Entities = new Constant<EntityStream>(EntityStream.Create(
                                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))

                                        ))
                                    }
                                }
                            }
                        }
                    },
                    $"Foo,Bar{Environment.NewLine}Hello,World{Environment.NewLine}Hello 2,World 2{Environment.NewLine}"
                );

                yield return new SequenceStepCase("Write Simple CSV with tab delimiter",

                    new Sequence
                    {
                        Steps = new[]
                        {
                            new Print<string>
                            {
                                Value = new FromStream
                                {
                                    Stream = new WriteCSV
                                    {
                                        Entities = new Constant<EntityStream>(EntityStream.Create(
                                            CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                                            CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))

                                        )),
                                        Delimiter = Constant("\t")
                                    }
                                }
                            }
                        }
                    },
                    $"Foo\tBar{Environment.NewLine}Hello\tWorld{Environment.NewLine}Hello 2\tWorld 2{Environment.NewLine}"
                );
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get
            {
                var expectedYaml = @"Do: WriteCSV
Entities:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')
Delimiter: ','
Encoding: EncodingEnum.Default";


                var step = CreateStepWithDefaultOrArbitraryValues();

                var case1 = new SerializeCase("Default", step.step, expectedYaml);

                yield return case1;
            }
        }
    }
}