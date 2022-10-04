using Reductech.Sequence.Core.Internal.Parser;

namespace Reductech.Sequence.Core.Tests;

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
                ("Syntax Error: mismatched input '<EOF>' expecting {'(', '[', AUTOMATICVARIABLE, VARIABLENAME, DATETIME, NUMBER, OPENISTRING, SIMPLEISTRING, MULTILINESTRING, DOUBLEQUOTEDSTRING, SINGLEQUOTEDSTRING, TRUE, FALSE, NULLVALUE, NAME}",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 1, Idx: 1 Text: <EOF>")
            );

            yield return new DeserializationErrorCase(
                "- 'hello'\r\n- 'world'",
                ("'InitialSteps[0]' cannot take the value 'hello'",
                 "hello - Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 8, Idx: 8 Text: 'hello'")
            );

            yield return new DeserializationErrorCase(
                "- 1 + 1\r\n- 2 + 2",
                ("Sequence expected Unit for parameter InitialSteps[0] but Step has type SCLInt",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 6, Idx: 6 Text: 1 + 1")
            );

            yield return new DeserializationErrorCase(
                "- <MyVar> = 1\r\n- print (stringtodate <MyVar>)",
                ("Variable 'MyVar' does not have type 'StringStream'.",
                 "Line: 2, Col: 22, Idx: 37 - Line: 2, Col: 28, Idx: 43 Text: <MyVar>")
            );

            yield return new DeserializationErrorCase(
                "<>",
                (
                    "The automatic variable was not set.",
                    "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 1, Idx: 1 Text: <>"
                )
            );

            yield return new DeserializationErrorCase(
                "MyMegaFunction true",
                ("The step 'MyMegaFunction' does not exist",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 18, Idx: 18 Text: MyMegaFunction true")
            );

            yield return new DeserializationErrorCase(
                "Print (2 + 2",
                ("Syntax Error: Unclosed Brackets",
                 "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 6, Idx: 6 Text: (")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<item> + 1))",
                ("The types 'ArrayOfUnknown' and 'StringStream' are incompatible.",
                 "Line: 1, Col: 31, Idx: 31 - Line: 1, Col: 36, Idx: 36 Text: <item>")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<Num> => <Num> + 1)) ",
                ("'Print.Value' cannot take the value 'Lambda'",
                 "Line: 1, Col: 30, Idx: 30 - Line: 1, Col: 49, Idx: 49 Text: (<Num> => <Num> + 1)")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two') (<Num> => Print <Num>) ", //The ) should be a ]
                (
                    @"Syntax Error: Unclosed Brackets",
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
                    "The types 'Entity' and 'StringStream' are incompatible.",
                    "Line: 2, Col: 16, Idx: 54 - Line: 2, Col: 22, Idx: 60 Text: <array>")
            );

            yield return new DeserializationErrorCase(
                "IncrementVariable 'hello' 1",
                ("'Variable' cannot take the value 'hello'",
                 "IncrementVariable - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 26, Idx: 26 Text: IncrementVariable 'hello' 1")
            );

            yield return new DeserializationErrorCase(
                "'Terms' cannot take the value '1'",
                ("Syntax Error: mismatched input 'cannot' expecting <EOF>",
                 "Line: 1, Col: 8, Idx: 8 - Line: 1, Col: 13, Idx: 13 Text: cannot")
            );

            //yield return new DeserializationErrorCase(
            //    "log <word>",
            //    ("Could not resolve variable 'word'",
            //     "Line: 1, Col: 4, Idx: 4 - Line: 1, Col: 9, Idx: 9 Text: <word>")
            //);

            //yield return new DeserializationErrorCase(
            //    "- log <word>\r\n- log <word>",
            //    ("Could not resolve variable 'word'",
            //     "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 11, Idx: 11 Text: <word>"),
            //    ("Could not resolve variable 'word'",
            //     "Line: 2, Col: 6, Idx: 20 - Line: 2, Col: 11, Idx: 25 Text: <word>")
            //);

            yield return new DeserializationErrorCase(
                "StringContains stringtrim 'abc' stringtrim ' b '",
                (
                    "StringTrim expected TrimSide for parameter Side but Step has type StringStream",
                    "Line: 1, Col: 32, Idx: 32 - Line: 1, Col: 47, Idx: 47 Text: stringtrim ' b '"
                ),
                ("Substring was missing or empty.",
                 "StringContains - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 47, Idx: 47 Text: StringContains stringtrim 'abc' stringtrim ' b '"
                )
            );

            yield return new DeserializationErrorCase(
                "Print <a>",
                ("Variable '<a>' does not exist.",
                 "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 8, Idx: 8 Text: <a>")
            );

            yield return new DeserializationErrorCase(
                "- Print <a>\r\n- [1,2,3] | foreach (Print <a>)",
                ("Variable '<a>' does not exist.",
                 "Line: 1, Col: 8, Idx: 8 - Line: 1, Col: 10, Idx: 10 Text: <a>"),
                ("Variable '<a>' does not exist.",
                 "Line: 2, Col: 27, Idx: 40 - Line: 2, Col: 29, Idx: 42 Text: <a>")
            );

            yield return new DeserializationErrorCase(
                "(Foo: 1)['bar']",
                ("The entity can never have a property named 'bar'.",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 14, Idx: 14 Text: (Foo: 1)['bar']")
            );

            yield return new DeserializationErrorCase(
                "StringToCase String: 'abc' TextCase.Upper",
                ("Syntax Error: Ordered arguments cannot appear after Named Arguments",
                 "Line: 1, Col: 27, Idx: 27 - Line: 1, Col: 34, Idx: 34 Text: TextCase")
            );

            yield return new DeserializationErrorCase(
                "StringToCase String: 'abc' 'Upper'",
                ("Syntax Error: Ordered arguments cannot appear after Named Arguments",
                 "Line: 1, Col: 27, Idx: 27 - Line: 1, Col: 33, Idx: 33 Text: 'Upper'")
            );

            yield return new DeserializationErrorCase(
                "(1 + 2",
                ("Syntax Error: Unclosed Brackets",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 0, Idx: 0 Text: (")
            );

            yield return new DeserializationErrorCase(
                "[1 + 2",
                ("Syntax Error: Unclosed Brackets",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 0, Idx: 0 Text: [")
            );

            yield return new DeserializationErrorCase(
                "log (1 + 2",
                ("Syntax Error: Unclosed Brackets",
                 "Line: 1, Col: 4, Idx: 4 - Line: 1, Col: 4, Idx: 4 Text: (")
            );

            yield return new DeserializationErrorCase(
                "1 + 2)",
                ("Syntax Error: Unopened Brackets",
                 "Line: 1, Col: 5, Idx: 5 - Line: 1, Col: 5, Idx: 5 Text: )")
            );

            yield return new DeserializationErrorCase(
                "log 1 + 2)",
                ("Syntax Error: Unopened Brackets",
                 "Line: 1, Col: 9, Idx: 9 - Line: 1, Col: 9, Idx: 9 Text: )")
            );

            yield return new DeserializationErrorCase(
                "log 123\n- log 456",
                ("Syntax Error: Sequences with multiple steps should start with a dash",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 6, Idx: 6 Text: log 123")
            );

            yield return new DeserializationErrorCase(
                "<",
                ("Syntax Error: Exception of type 'Antlr4.Runtime.InputMismatchException' was thrown.",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 0, Idx: 0 Text: <")
            );

            yield return new DeserializationErrorCase(
                "- <myVar> = 2\n- log <myVar>",
                ("The variable 'myVar' was injected and therefore cannot be set.",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 12, Idx: 12 Text: <myVar> = 2")
            ).WithInjectedVariable(
                new VariableName("myVar"),
                ISCLObject.CreateFromCSharpObject(1)
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

        private Dictionary<VariableName, InjectedVariable> VariablesToInject { get; set; } =
            new();

        /// <inheritdoc />
        public void Run(ITestOutputHelper testOutputHelper)
        {
            testOutputHelper.WriteLine(SCL);

            var sfs = StepFactoryStore.Create();

            var result = SCLParsing.TryParseStep(SCL)
                .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs, VariablesToInject))
                .Map(SCLRunner.ConvertToUnitStep);

            result.IsFailure.Should().BeTrue("Case should fail");

            var realErrorPairs =
                result.Error.GetAllErrors()
                    .Select(x => (x.Message, x.Location.AsString()))
                    .ToArray();

            realErrorPairs.Should().BeEquivalentTo(ExpectedErrors);
        }

        public string Name => SCL;

        public DeserializationErrorCase WithInjectedVariable(
            VariableName variableName,
            ISCLObject sclObject)
        {
            VariablesToInject[variableName] = new InjectedVariable(sclObject, null);
            return this;
        }
    }
}
