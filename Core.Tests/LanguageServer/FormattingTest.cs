using Reductech.Sequence.Core.LanguageServer;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class FormattingTest
{
    [Theory]
    [InlineData("Print 123", "Print Value: 123 [start: (0, 0), end: (0, 9)]")]
    [InlineData(
        "- Print 123\r\n- a b\r\n- Print 456",
        "- Print Value: 123 [start: (0, 0), end: (0, 11)]",
        "- Print Value: 456 [start: (2, 0), end: (2, 11)]"
    )]
    [InlineData(
        "RestGetJson baseurl: 'abc' #First Comment\r\nrelativeurl: 'def' /*Second Comment*/headers: (a: 1, b: 2)",
        @"RESTGetJSON
	BaseURL: ""abc""
	RelativeURL: ""def""
	Headers: (a: 1,b: 2)
#First Comment
/*Second Comment*/ [start: (0, 0), end: (1, 58)]"
    )]
    [InlineData(
        "print (StringToCase 'ab' /* my comment */ textcase.upper)",
        @"Print Value: (StringToCase
	String: ""ab"" /* my comment */
	Case: TextCase.Upper
)"
    )]
    [InlineData(
        "foreach ['a','b', 'c'] (<v> => print (StringToCase <v> TextCase.Upper))",
        @"ForEach
	Array: [""a"", ""b"", ""c""]
	Action: (<v> => (Print Value: (StringToCase
		String: <v>
		Case: TextCase.Upper
	)))"
    )]
    public void ShouldGiveCorrectFormatting(string text, params string[] expectedFormattings)
    {
        var sfs = StepFactoryStore.Create();

        var textEdits = FormattingHelper.FormatSCL(text, sfs);

        var actual = textEdits.Select(
                x =>
                    $"{x.NewText} [start: ({x.StartLine}, {x.StartColumn}), end: ({x.EndLine}, {x.EndColumn})]"
            )
            .ToList();

        actual.Should().BeEquivalentTo(expectedFormattings);
    }
}
