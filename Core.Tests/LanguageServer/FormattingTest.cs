﻿using Sequence.Core.LanguageServer;

namespace Sequence.Core.Tests.LanguageServer;

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
        "HttpRequest uri: 'abc' #First Comment\r\nheaders: (a: 1, b: 2)",
        @"HttpRequest
Uri    : ""abc"" #First Comment
Headers: (
	'a': 1
	'b': 2
) [start: (0, 0), end: (1, 21)]"
    )]
    [InlineData(
        "print (StringToCase 'ab' /* my comment */ textcase.upper)",
        @"Print Value: (StringToCase
String: ""ab""
Case  : TextCase.Upper)/* my comment */ [start: (0, 0), end: (0, 57)]"
    )]
    [InlineData(
        "foreach ['a','b', 'c'] (<v> => print (StringToCase <v> TextCase.Upper))",
        @"ForEach
Array : [""a"", ""b"", ""c""]
	Action: (<v> => Print Value: (StringToCase
	String: (<v>)
Case  : TextCase.Upper)) [start: (0, 0), end: (0, 71)]"
    )]
    [InlineData(
        @"- (
  a: 1, #a
  b: 1, #b
  c: 1, #c
  d: 1 #d
)",
        @"- (
	'a': 1#a
	'b': 1#b
	'c': 1#c
	'd': 1#d
) [start: (0, 0), end: (5, 1)]"
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
