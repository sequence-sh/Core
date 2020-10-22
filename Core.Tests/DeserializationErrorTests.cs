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
                yield return new DeserializationErrorCase("", ("Yaml is empty.", EntireSequenceLocation.Instance.AsString));

                yield return new DeserializationErrorCase("\"Print(Value = 123)\"", ("Yaml must represent a step with return type Unit", "Print(Value = 123)"));

                yield return new DeserializationErrorCase("'Print(Value = 123)'", ("Yaml must represent a step with return type Unit", "Print(Value = 123)"));

                yield return new DeserializationErrorCase("Do:Nothing",("Could not tokenize 'Do:Nothing'", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 11, Idx: 10"));
                yield return new DeserializationErrorCase("Do: Print\nValue:Hello",
                    ("While scanning a simple key, could not find expected ':'.", "Line: 3, Col: 1, Idx: 21 - Line: 3, Col: 1, Idx: 21"));


                yield return new DeserializationErrorCase("Do: Print\nWord: Hello\nWord: World\nText: Goodbye",
                    ("Duplicate Parameter 'Word'", "Line: 1, Col: 1, Idx: 0 - Line: 4, Col: 5, Idx: 38"), ("Missing Parameter 'Value' in 'Print'", "Line: 1, Col: 1, Idx: 0 - Line: 5, Col: 1, Idx: 47")
                    );

                yield return new DeserializationErrorCase("Do: Print\nDo: Print\nValue: Hello",
                    ("Duplicate Parameter 'Do'", "Line: 1, Col: 1, Idx: 0 - Line: 3, Col: 6, Idx: 25")
                );
                yield return new DeserializationErrorCase("Do: Print\nValue: Hello\nConfig:\n\tDoNotSplit: false\nConfig:\n\tDoNotSplit: true",
                    ("Duplicate Parameter 'Config'", "Line: 1, Col: 1, Idx: 0 - Line: 7, Col: 1, Idx: 75")
                    );


                yield return new DeserializationErrorCase("Da: Print\nValue: Hello",
                    ("Missing Parameter 'Do' in 'Step Definition'", "Line: 1, Col: 1, Idx: 0 - Line: 3, Col: 1, Idx: 22")
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


                yield return new DeserializationErrorCase(@"- <CsvHeader> = ['a', 'b']
- <SearchTerms> = ReadCsv(Text = ReadFile(Folder = <CurrentDir>, FileName = <SearchTagCSV>, ColumnsToMap = <CsvHeader>))",
("Missing Parameter 'ColumnsToMap' in 'ReadCsv'", "Line: 2, Col: 3, Idx: 30 - Line: 2, Col: 121, Idx: 148"),
("Unexpected Parameter 'ColumnsToMap' in 'ReadFile'", "Line: 2, Col: 3, Idx: 30 - Line: 2, Col: 121, Idx: 148")
                    );

                yield return new DeserializationErrorCase(@"
- <ArrayVar1> = Array(Elements = ['abc', '123'])
- <ArrayVar2> = Array(Elements = ['abc', '123'])
- Print(Value = (<ArrayVar1> == <ArrayVar2>))",
                    ("Cannot compare objects of type 'ListOfString'", "<ArrayVar1> == <ArrayVar2>"));

                yield return new DeserializationErrorCase("MyMegaFunction(Value = true)", ("The step 'MyMegaFunction' does not exist", "Line: 1, Col: 1, Idx: 0 - Line: 1, Col: 29, Idx: 28"));

                yield return new DeserializationErrorCase(@"- >
  ForEach(
    Array = ['a','b','c'],
    VariableName = <char>,
    Action = Print(Value = <char>)
  )",
                    ("'ForEach( Array = ['a','b','c'], VariableName = <char>, Action = Print(Value = <char>) )' is a 'String' but it should be a 'Unit' to be a member of 'Sequence'", "Sequence")
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
