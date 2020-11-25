using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        [Theory]
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

                yield return new DeserializationErrorCase("- <Entity> = 123\n- Print(Value = <Entity>)",
                    ("The VariableName <Entity> is Reserved.", "<Entity> = 123")
                    );


                yield return new DeserializationErrorCase("- <ReductechEntity> = 123\n- Print(Value = <ReductechEntity>)",
                    ("The VariableName Prefix 'Reductech' is Reserved.", "<ReductechEntity> = 123")
                    );

                yield return new DeserializationErrorCase("", ("Yaml is empty.", EntireSequenceLocation.Instance.AsString));

                yield return new DeserializationErrorCase("\"Print(Value = 123)\"", ("Yaml must represent a step with return type Unit", "Print(Value = 123)"));

                yield return new DeserializationErrorCase("'Print(Value = 123)'", ("Yaml must represent a step with return type Unit", "Print(Value = 123)"));

                yield return new DeserializationErrorCase("Do:Nothing",("unexpected `D`", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 11, Idx: 10"));

                yield return new DeserializationErrorCase("Do: Print\nValue:Hello",
                    ("While scanning a simple key, could not find expected ':'.", "Line: 2, Col: 1, Idx: 10 - Line: 2, Col: 1, Idx: 10"));


                yield return new DeserializationErrorCase("Do: Print\nWord: Hello\nWord: World\nText: Goodbye",
                    ("Duplicate Parameter 'Word'", "Line: 1, Col: 1, Idx: 0 - Line: 4, Col: 5, Idx: 38")//, ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 5, Col: 1, Idx: 47")
                    );

                yield return new DeserializationErrorCase("Do: Print\nDo: Print\nValue: Hello",
                    ("Unexpected Parameter 'Do' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 4, Col: 1, Idx: 32")
                );
                yield return new DeserializationErrorCase("Do: Print\nValue: Hello\nConfig:\n DoNotSplit: false\nConfig:\n DoNotSplit: true",
                    ("Duplicate Parameter 'Config'", "Line: 1, Col: 1, Idx: 0 - Line: 7, Col: 1, Idx: 75")
                    );


                yield return new DeserializationErrorCase("Da: Print\nValue: Hello",
                    ("Could not deserialize an object whose first property is 'Da'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 10, Idx: 9")
                );

                yield return new DeserializationErrorCase("Print(Word = 'hello', Term = 'world')",
                    ("Unexpected Parameter 'Word' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 38, Idx: 37"),
                ("Unexpected Parameter 'Term' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 38, Idx: 37"),
                ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 38, Idx: 37"));


                yield return new DeserializationErrorCase("Compare(Left = Print(Foo = 1), Right = Print(Foo = 2), Operator = Print(Foo = 2))",
                    ("Unexpected Parameter 'Foo' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81"),
                ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81"),
                ("Unexpected Parameter 'Foo' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81"),
                ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81"),
                ("Unexpected Parameter 'Foo' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81"),
                ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 82, Idx: 81")
                );



                yield return new DeserializationErrorCase("- <ArrayVar1> = Array(Elements = ['abc', '123'])\n- <ArrayVar2> = Array(Elements = ['abc', '123'])\n- Print(Value = (<ArrayVar1> == <ArrayVar2>))",
                    ("Cannot compare objects of type 'ListOfString'", "<ArrayVar1> == <ArrayVar2>"));

                yield return new DeserializationErrorCase("MyMegaFunction(Value = true)", ("The step 'MyMegaFunction' does not exist", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 29, Idx: 28"));

                yield return new DeserializationErrorCase("- >\n  ForEach(\n    Array = ['a','b','c'],\n    VariableName = <char>,\n    Action = Print(Value = <char>))",
                    ("'ForEach( Array = ['a','b','c'], VariableName = <char>, Action = Print(Value = <char>))' is a 'String' but it should be a 'Unit' to be a member of 'Sequence'", "Sequence")
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

                var result = YamlMethods.DeserializeFromYaml(Name, sfs)
                    .Bind(x=>x.TryFreeze())
                    .Bind(YamlRunner.ConvertToUnitStep);

                result.ShouldBeFailure();

                var realErrorPairs =
                    result.Error.GetAllErrors().Select(x => (x.Message, x.Location.AsString)).ToArray();

                realErrorPairs.Should().BeEquivalentTo(_expectedErrors);


            }
        }

    }
}
