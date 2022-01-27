using Reductech.Sequence.Core.LanguageServer;
using Reductech.Sequence.Core.LanguageServer.Objects;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class CompletionTest
{
    public const string ErrorText = @"- FileRead 'artwork_data.csv'
- 0.1.2.3";

    [Theory]
    [InlineData("- ",           1, 2,  null)]
    [InlineData("Print 123",    1, 6,  "Value")]
    [InlineData("- Print  ",    0, 11, "Value")]
    [InlineData("Print  ",      0, 9,  "Value")]
    [InlineData("Print P",      0, 8,  "Value")]
    [InlineData("Print\r\nP",   1, 0,  "Value")]
    [InlineData("- Print\r\nP", 1, 0,  "Value")]
    [InlineData("- Print P",    0, 8,  "Value")]
    [InlineData(LongText,       1, 3,  "ArrayFilter")]
    public void ShouldGiveCorrectCompletion(
        string text,
        int line,
        int character,
        string? expectedLabel)
    {
        var sfs = StepFactoryStore.Create();

        var visitor = new CompletionVisitor(new LinePosition(line, character), sfs);

        var completionList = visitor.LexParseAndVisit(
            text,
            x => x.RemoveErrorListeners(),
            x => x.RemoveErrorListeners()
        );

        if (string.IsNullOrWhiteSpace(expectedLabel))
            completionList.Should().BeNull();

        else
        {
            completionList.Should().NotBeNull();

            var labels =
                completionList.Items.Select(x => x.Label);

            labels.Should().Contain(expectedLabel);

            foreach (var item in completionList.Items)
            {
                item.Documentation.Should().NotBeNull();
                item.Documentation.Should().NotBeEmpty();
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
