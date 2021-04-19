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
                "Print Value: 'Hello' Value: 'World'",
                ("Duplicate Parameter: Value.",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 34, Idx: 34 Text: Print Value: 'Hello' Value: 'World'")
            );

            yield return new DeserializationErrorCase(
                "Print Value: 'hello' Term: 'world'",
                ("Unexpected Parameter 'Term' in 'Print'",
                 "Print - Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 33, Idx: 33 Text: Print Value: 'hello' Term: 'world'")
            );

            yield return new DeserializationErrorCase(
                "1/",
                ("Syntax Error: mismatched input '<EOF>' expecting {'(', '[', VARIABLENAME, DATETIME, NUMBER, OPENISTRING, SIMPLEISTRING, DOUBLEQUOTEDSTRING, SINGLEQUOTEDSTRING, TRUE, FALSE, NAME}",
                 "Line: 1, Col: 2, Idx: 2 - Line: 1, Col: 1, Idx: 1 Text: <EOF>")
            );

            //yield return new DeserializationErrorCase(
            //    "Print(['abc', '123'] == ['abc', '123'])",
            //    ("Type ArrayOfStringStream is not comparable and so cannot be used for sorting.",
            //     "Line: 1, Col: 6, Idx: 6 - Line: 1, Col: 37, Idx: 37 Text: ['abc', '123'] == ['abc', '123']")
            //);

            yield return new DeserializationErrorCase(
                "MyMegaFunction true",
                ("The step 'MyMegaFunction' does not exist",
                 "Line: 1, Col: 0, Idx: 0 - Line: 1, Col: 18, Idx: 18 Text: MyMegaFunction true")
            );

            yield return new DeserializationErrorCase(
                "Print (2 + 2",
                ("Syntax Error: missing ')' at '<EOF>'",
                 "Line: 1, Col: 12, Idx: 12 - Line: 1, Col: 11, Idx: 11 Text: <EOF>")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<Entity> + 1))",
                ("IntConstant has output type Integer, not String",
                 "1 - Line: 1, Col: 42, Idx: 42 - Line: 1, Col: 42, Idx: 42 Text: 1")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two'] (Print (<Num> + 1)) <Num>",
                ("IntConstant has output type Integer, not String",
                 "1 - Line: 1, Col: 39, Idx: 39 - Line: 1, Col: 39, Idx: 39 Text: 1")
            );

            yield return new DeserializationErrorCase(
                "Foreach ['one', 'two') (Print <Num>) <Num>", //The ) should be a ]
                (
                    @"Syntax Error: extraneous input ')' expecting {'(', '[', ']', ',', VARIABLENAME, DATETIME, NUMBER, OPENISTRING, SIMPLEISTRING, DOUBLEQUOTEDSTRING, SINGLEQUOTEDSTRING, TRUE, FALSE, NAME}",
                    @"Line: 1, Col: 21, Idx: 21 - Line: 1, Col: 21, Idx: 21 Text: )"),
                (@"Syntax Error: mismatched input '<EOF>' expecting {'(', '[', ']', ',', VARIABLENAME, DATETIME, NUMBER, OPENISTRING, SIMPLEISTRING, DOUBLEQUOTEDSTRING, SINGLEQUOTEDSTRING, TRUE, FALSE, NAME}",
                 @"Line: 1, Col: 42, Idx: 42 - Line: 1, Col: 41, Idx: 41 Text: <EOF>"
                )
            );

            yield return new DeserializationErrorCase(
                "- <Input> = [\r\n    (prop1: \"\"value1\"\" prop2: 2),\r\n    (prop1: \"\"value3\"\" prop2: 4),\r\n  ]",
                (@"Syntax Error: No Viable Alternative - '""""' was unexpected.",
                 @"Line: 2, Col: 20, Idx: 35 - Line: 2, Col: 21, Idx: 36 Text: """"")
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
            var sfs = StepFactoryStore.CreateFromAssemblies();

            var result = SCLParsing.ParseSequence(SCL)
                .Bind(x => x.TryFreeze(TypeReference.Any.Instance, sfs))
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
