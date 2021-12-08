using System.Collections.Generic;
using System.Linq;
using AutoTheory;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Xunit.Abstractions;

namespace Reductech.EDR.Core.Tests
{

public partial class DeserializationErrorTests
{
    [GenerateTheory("DeserializationError")]
    protected IEnumerable<ITestInstance> TestCases
    {
        get
        {
            yield return new DeserializationErrorCase(
                "",
                ("SCL is empty.", ErrorLocation.EmptyLocation.AsString())
            );

            yield return new DeserializationErrorCase(
                "- EntityGetValue [1,2] 'a'",
                ("EntityGetValue expected Entity for parameter Entity but [1,2] has type Array/Sequence",
                 "Line: 1, Col: 17, Idx: 17 - Line: 1, Col: 21, Idx: 21 Text: [1,2]")
            );

            yield return new DeserializationErrorCase(
                "Print Value: 'Hello' Value: 'World'",
                ("Duplicate Parameter: Value.",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 34, Idx: 34 Text: Print Value: 'Hello' Value: 'World'")
            );

            yield return new DeserializationErrorCase(
                "Print Value: 'hello' Term: 'world'",
                ("Unexpected Parameter 'Term' in 'Print'",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 33, Idx: 33 Text: Print Value: 'hello' Term: 'world'")
            );

            yield return new DeserializationErrorCase(
                "1/",
                ("Syntax Error: mismatched input '<EOF>' expecting {'(', '[', AUTOMATICVARIABLE, VARIABLENAME, DATETIME, NUMBER, OPENISTRING, SIMPLEISTRING, DOUBLEQUOTEDSTRING, SINGLEQUOTEDSTRING, TRUE, FALSE, NULLVALUE, NAME}",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 1, Idx: 1 Text: <EOF>")
            );

            yield return new DeserializationErrorCase(
                "- 'hello'\r\n- 'world'",
                ("'InitialSteps[0]' cannot take the value 'hello'",
                 "Sequence - Line: 1, Col: 0, Idx: 0 - Line: 2, Col: 8, Idx: 19 Text: - 'hello'\r\n- 'world'")
            );

            yield return new DeserializationErrorCase(
                "- 1 + 1\r\n- 2 + 2",
                ("Sequence expected Unit for parameter InitialSteps[0] but Step has type Integer",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 6, Idx: 6 Text: 1 + 1")
            );

            yield return new DeserializationErrorCase(
                "- <MyVar> = 1\r\n- print (stringtodate <MyVar>)",
                ("Variable 'MyVar' does not have type 'String'.",
                 "Line: 2, Col: 22, Idx: 37 - Line: 2, Col: 28, Idx: 43 Text: <MyVar>")
            );

            yield return new DeserializationErrorCase(
                "<>",
                (
                    "The automatic variable was not set.",
                    "GetAutomaticVariable - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 1, Idx: 1 Text: <>"
                )
            );

            yield return new DeserializationErrorCase(
                "MyMegaFunction true",
                ("The step 'MyMegaFunction' does not exist",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 18, Idx: 18 Text: MyMegaFunction true")
            );

            yield return new DeserializationErrorCase(
                "Print (2 + 2",
                ("Syntax Error: mismatched input '(' expecting <EOF>",
                 "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 6, Idx: 6 Text: (")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<item> + 1))",
                ("StringJoin expected String for parameter Strings but Step has type Integer",
                 "1 - Line: 1, Col: 40, Idx: 40 - Line: 1, Col: 40, Idx: 40 Text: 1")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<Num> => <Num> + 1)) ",
                ("Could not infer type - <Num> could not be inferred",
                 "Line: 1, Col: 40, Idx: 40 - Line: 1, Col: 44, Idx: 44 Text: <Num>"),
                ("StringJoin expected String for parameter Strings but Step has type Integer",
                 "1 - Line: 1, Col: 48, Idx: 48 - Line: 1, Col: 48, Idx: 48 Text: 1")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two') (<Num> => Print <Num>) ", //The ) should be a ]
                (
                    @"Syntax Error: mismatched input '[' expecting <EOF>",
                    @"Line: 1, Col: 8, Idx: 8 - Line: 1, Col: 8, Idx: 8 Text: [")
            );

            yield return new DeserializationErrorCase(
                "- <Input> = [\r\n    (prop1: \"\"value1\"\" prop2: 2),\r\n    (prop1: \"\"value3\"\" prop2: 4),\r\n  ]",
                (@"Syntax Error: No Viable Alternative - '""""' was unexpected.",
                 @"Line: 2, Col: 20, Idx: 35 - Line: 2, Col: 21, Idx: 36 Text: """"")
            );

            yield return new DeserializationErrorCase(
                "- <array> = [('Foo': 1), ('Foo': 2)]\r\n- StringIsEmpty <array>[0]",
                (
                    "Variable 'array' does not have type 'Entity'.",
                    "Line: 2, Col: 16, Idx: 54 - Line: 2, Col: 22, Idx: 60 Text: <array>")
            );

            yield return new DeserializationErrorCase(
                "IncrementVariable 'hello' 1",
                ("'Variable' cannot take the value 'hello'",
                 "IncrementVariable - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 26, Idx: 26 Text: IncrementVariable 'hello' 1")
            );

            yield return new DeserializationErrorCase(
                "Sum 1",
                ("Sum expected ArrayOfInteger for parameter Terms but Step has type Integer",
                 "1 - Line: 1, Col: 4, Idx: 4 - Line: 1, Col: 4, Idx: 4 Text: 1")
            );

            yield return new DeserializationErrorCase(
                "log <word>",
                ("Could not resolve variable 'word'",
                 "Line: 1, Col: 4, Idx: 4 - Line: 1, Col: 9, Idx: 9 Text: <word>")
            );

            yield return new DeserializationErrorCase(
                "- log <word>\r\n- log <word>",
                ("Could not resolve variable 'word'",
                 "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 11, Idx: 11 Text: <word>"),
                ("Could not resolve variable 'word'",
                 "Line: 2, Col: 6, Idx: 20 - Line: 2, Col: 11, Idx: 25 Text: <word>")
            );

            yield return new DeserializationErrorCase(
                "StringContains stringtrim 'abc' stringtrim ' b '",
                (
                    "StringTrim expected TrimSide for parameter Side but Step has type String",
                    "Line: 1, Col: 32, Idx: 32 - Line: 1, Col: 47, Idx: 47 Text: stringtrim ' b '"
                ),
                ("Substring was missing or empty.",
                 "StringContains - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 47, Idx: 47 Text: StringContains stringtrim 'abc' stringtrim ' b '"
                )
            );
        }
    }

    private class DeserializationErrorCase : ITestInstance
    {
        public DeserializationErrorCase(
            string scl,
            params (string error, string location)[] expectedErrors)
        {
            SCL            = scl;
            ExpectedErrors = expectedErrors;
        }

        public string SCL { get; set; }
        public (string error, string location)[] ExpectedErrors { get; set; }

        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(SCL);

            var sfs = StepFactoryStore.Create();

            var result = SCLParsing.TryParseStep(SCL)
                .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs))
                .Map(SCLRunner.ConvertToUnitStep);

            result.IsFailure.Should().BeTrue("Case should fail");

            var realErrorPairs =
                result.Error.GetAllErrors()
                    .Select(x => (x.Message, x.Location.AsString()))
                    .ToArray();

            realErrorPairs.Should().BeEquivalentTo(ExpectedErrors);
        }

        public string Name => SCL;
    }
}

}
