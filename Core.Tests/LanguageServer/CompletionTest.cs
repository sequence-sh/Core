using Sequence.Core.Internal.Documentation;
using Sequence.Core.LanguageServer;
using Sequence.Core.LanguageServer.Objects;

namespace Sequence.Core.Tests.LanguageServer;

public class CompletionTest
{
    [Theory]
    [InlineData("- ",                                         0, 2,  null,          null)]
    [InlineData("(Foo: 1).Fo",                                0, 10, "Foo",         9)]
    [InlineData("(Foo: (bar:1, baz: 2)).Fo",                  0, 25, "Foo.bar",     23)]
    [InlineData("- Print  ",                                  0, 11, "Value",       null)]
    [InlineData("Print  ",                                    0, 9,  "Value",       null)]
    [InlineData("Print Value:123",                            0, 9,  null,          null)]
    [InlineData("Print Value: (Ad)",                          0, 15, "Add",         14)]
    [InlineData("Print P",                                    0, 8,  "Value",       null)]
    [InlineData("Print\r\nV",                                 1, 0,  "Value",       null)]
    [InlineData("- Print\r\nV",                               1, 0,  "Value",       null)]
    [InlineData("- Print P",                                  1, 8,  "Value",       null)]
    [InlineData("- Print 123\r\n- HttpRequest Uri: 'abc' h",  1, 26, "Headers",     25)]
    [InlineData("- Print ...\r\n- HttpRequest Uri: 'abc' h",  1, 26, "Headers",     25)]
    [InlineData("- Print 123\r\n- HttpRequest Uri: 'abc' he", 1, 27, "Headers",     25)]
    [InlineData(LongText,                                     1, 2,  "ArrayFilter", null)]
    [InlineData("<",                                          0, 1,  "<var1>",      0)]
    [InlineData("<v",                                         0, 1,  "<var1>",      0)]
    [InlineData("- <v",                                       0, 4,  "<var1>",      2)]
    //[InlineData("- <myvar> = 1\r\n- <m",                      1, 4,  "<myvar>",     2)] //TODO add this test back
    public void ShouldGiveCorrectCompletion(
        string text,
        int line,
        int character,
        string? expectedLabel,
        int? expectedStartColumn)
    {
        var sfs = StepFactoryStore.Create();

        var completionList =
            CompletionHelper.GetCompletionResponse(
                text,
                new LinePosition(line, character),
                sfs,
                DocumentationOptions.DefaultDocumentationOptionsHtml,
                new Dictionary<VariableName, InjectedVariable>()
                {
                    {
                        new VariableName("var1"),
                        new InjectedVariable(SCLBool.True, "Amazing Description")
                    }
                }
            );

        if (string.IsNullOrWhiteSpace(expectedLabel))
            completionList.Items.Should().BeEmpty();

        else
        {
            var labels =
                completionList.Items.Select(x => x.Label);

            labels.Should().Contain(expectedLabel);

            foreach (var item in completionList.Items)
            {
                item.Documentation.Should().NotBeNull();
                item.Documentation.Should().NotBeEmpty();

                item.TextEdit.StartLine.Should().Be(line);
                item.TextEdit.StartColumn.Should().Be(expectedStartColumn ?? character);
            }
        }
    }

    public const string LongText = @"[(artist: 'Blake, Robert' artistid: 123)]
| ArrayFilter ((from <entity> 'artist') == 'Blake, Robert')
| EntityMap (in <entity> 'artist' (StringToCase (from <entity> 'artist') TextCase.Upper ))
| EntityMapProperties (artist: 'Artist Name' artistId: 'ArtistId')
| ArraySort (from <entity> 'year')
| ArrayDistinct (from <entity> 'id')
| print
";
}
