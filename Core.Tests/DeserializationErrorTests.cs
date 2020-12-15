using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

    public class DeserializationErrorTests : DeserializationErrorTestCases
    {
        public DeserializationErrorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory()]
        [ClassData(typeof(DeserializationErrorTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class DeserializationErrorTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases
        {
            get
            {
                yield return new DeserializationErrorCase("", ("Sequence is empty.", EntireSequenceLocation.Instance.AsString));

                yield return new DeserializationErrorCase("- <Entity> = 123\n- Print <Entity>",
                    ("The Variable <Entity> is Reserved.", "<Entity> = 123"));

                yield return new DeserializationErrorCase("\"Print 123\"", ("Yaml must represent a step with return type Unit", "\"Print 123\""));

                yield return new DeserializationErrorCase("'Print 123'", ("Yaml must represent a step with return type Unit", "'Print 123'"));


                yield return new DeserializationErrorCase("Print Value: Hello Value: World",
                    ("Duplicate Parameter 'Word'", "Line: 1, Col: 1, Idx: 0 - Line: 4, Col: 5, Idx: 38")
                    );

                yield return new DeserializationErrorCase("Print Value: 'hello' Term: 'world')",
                ("Unexpected Parameter 'Term' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 38, Idx: 37"));

                yield return new DeserializationErrorCase("Print(['abc', '123'] == ['abc', '123'])",
                    ("Cannot compare objects of type 'ListOfString'", "['abc', '123'] == ['abc', '123']"));

                yield return new DeserializationErrorCase("MyMegaFunction true", ("The step 'MyMegaFunction' does not exist", "MyMegaFunction true"));

                yield return new DeserializationErrorCase("ForEach ['a','b','c'] <char> (Print <char>)",
                    ("'ForEach( Array = ['a','b','c'], Variable = <char>, Action = Print(Value = <char>))' is a 'String' but it should be a 'Unit' to be a member of 'Sequence'",
                        "ForEach ['a','b','c'] <char> (Print <char>)")
                );

            }
        }


        private class DeserializationErrorCase : ITestBaseCase
        {
            private readonly (string error, string location)[] _expectedErrors;

            public DeserializationErrorCase(string yaml, params (string error, string location)[] expectedErrors)
            {
                Name = yaml;
                _expectedErrors = expectedErrors;
            }

            /// <inheritdoc />
            public string Name { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IFreezableStep));

                var result = SequenceParsing.ParseSequence(Name)
                    .Bind(x=>x.TryFreeze(sfs))
                    .Bind(SequenceRunner.ConvertToUnitStep);

                result.ShouldBeFailure();

                var realErrorPairs =
                    result.Error.GetAllErrors().Select(x => (x.Message, x.Location.AsString)).ToArray();

                realErrorPairs.Should().BeEquivalentTo(_expectedErrors);


            }
        }

    }
}
