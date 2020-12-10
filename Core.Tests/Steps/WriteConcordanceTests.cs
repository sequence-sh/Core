using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Core.Tests.Steps
{
    public class WriteConcordanceTests : StepTestBase<ToConcordance, StringStream>
    {
        /// <inheritdoc />
        public WriteConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new StepCase("Write Simple Concordance",

                    new Print<StringStream>
                    {
                        Value = new ToConcordance()
                        {
                            Entities =  Constant(EntityStream.Create(
                                    CreateEntity(("Foo", "Hello"), ("Bar", "World")),
                                    CreateEntity(("Foo", "Hello 2"), ("Bar", "World 2"))

                                ))
                        }
                    }, Unit.Default,
                    $"þFooþþBarþ{ Environment.NewLine }þHelloþþWorldþ{ Environment.NewLine }þHello 2þþWorld 2þ{Environment.NewLine}"
                );

                yield return new StepCase("Write Simple Concordance MultiValue",

                    new Print<StringStream>
                    {
                        Value = new ToConcordance
                        {
                            Entities =  Constant(EntityStream.Create(

                                    new Entity(
                                        new KeyValuePair<string, EntityValue>("Foo", EntityValue.Create("Hello")),
                                        new KeyValuePair<string, EntityValue>("Bar", EntityValue.Create(new[] { "World", "Earth" }))),
                                    new Entity(
                                        new KeyValuePair<string, EntityValue>("Foo", EntityValue.Create("Hello 2")),
                                        new KeyValuePair<string, EntityValue>("Bar", EntityValue.Create(new[] { "World 2", "Earth 2" })))
                                ))
                        }
                    }, Unit.Default,
                    $"þFooþþBarþ{ Environment.NewLine }þHelloþþWorld|Earthþ{ Environment.NewLine }þHello 2þþWorld 2|Earth 2þ{Environment.NewLine}"
                );
            }
        }

//        /// <inheritdoc />
//        protected override IEnumerable<SerializeCase> SerializeCases
//        {
//            get
//            {
//                var expectedYaml = @"Do: ToConcordance
//Entities:
//- (Prop1 = 'Val0',Prop2 = 'Val1')
//- (Prop1 = 'Val2',Prop2 = 'Val3')
//- (Prop1 = 'Val4',Prop2 = 'Val5')
//Encoding: EncodingEnum.UTF8
//Delimiter: ""\x14""
//QuoteCharacter: 'þ'
//AlwaysQuote: True
//MultiValueDelimiter: '|'
//DateTimeFormat: 'O'";


//                var step = CreateStepWithDefaultOrArbitraryValuesAsync();

//                var case1 = new SerializeCase("Default", step.step, expectedYaml);

//                yield return case1;
//            }
//        }

    }
}