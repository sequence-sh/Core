using Reductech.Sequence.Core.LanguageServer;
using Reductech.Sequence.Core.LanguageServer.Objects;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class CompletionTest
{
    [Theory]
    [InlineData("- ",                                         0, 2,  null,          null)]
    [InlineData("- Print  ",                                  0, 11, "Value",       null)]
    [InlineData("Print  ",                                    0, 9,  "Value",       null)]
    [InlineData("Print P",                                    0, 8,  "Value",       null)]
    [InlineData("Print\r\nV",                                 1, 0,  "Value",       null)]
    [InlineData("- Print\r\nV",                               1, 0,  "Value",       null)]
    [InlineData("- Print P",                                  1, 8,  "Value",       null)]
    [InlineData("- Print 123\r\n- HttpRequest Uri: 'abc' h",  1, 26, "Headers",     25)]
    [InlineData("- Print ...\r\n- HttpRequest Uri: 'abc' h",  1, 26, "Headers",     25)]
    [InlineData("- Print 123\r\n- HttpRequest Uri: 'abc' he", 1, 27, "Headers",     25)]
    [InlineData(LongText,                                     1, 2,  "ArrayFilter", null)]
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
                sfs
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
