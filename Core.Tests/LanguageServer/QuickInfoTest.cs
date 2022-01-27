using Reductech.Sequence.Core.LanguageServer;
using Reductech.Sequence.Core.LanguageServer.Objects;

namespace Reductech.Sequence.Core.Tests.LanguageServer;

public class QuickInfoTest
{
    public const string LongText = @"[(artist: 'Blake, Robert' artistid: 123)]
| ArrayFilter ((from <entity> 'artist') == 'Blake, Robert')
| EntityMap (in <entity> 'artist' (StringToCase (from <entity> 'artist') TextCase.Upper ))
| EntityMapProperties (artist: 'Artist Name' artistId: 'ArtistId')
| ArraySort (from <entity> 'year')
| ArrayDistinct (from <entity> 'id')
| print
";

    [Theory]
    [InlineData("Print 123", 0, 1, "`Print`", "`Unit`", "Prints a value to the console.")]
    [InlineData("Print 123", 0, 8, "`123`",   "`SCLInt`")]
    [InlineData(
        "- Print 123\r\n- a b",
        0,
        4,
        "`Print`",
        "`Unit`",
        "Prints a value to the console."
    )]
    [InlineData("- a .", 0, 3, "Syntax Error: no viable alternative at input '- a .'")]
    //[InlineData("- Print 123\r\n- a b", 1 ,1, "Syntax Error: no viable alternative at input '- a b'" )]
    [InlineData("- <val> = 123\r\n- print <val>", 1, 9,  "`<val>`",           "`SCLInt`")]
    [InlineData(LongText,                         0, 12, "`'Blake, Robert'`", "`StringStream`")]
    [InlineData(
        LongText,
        1,
        3,
        "`ArrayFilter`",
        "`Array of T`",
        "Filter an array or entity stream using a conditional statement"
    )]
    [InlineData(
        LongText,
        1,
        14,
        "`Predicate`",
        "`T`",
        "A function that determines whether an entity should be included."
    )]
    public void ShouldGiveCorrectHover(
        string text,
        int line,
        int character,
        params string[] expectedHovers)
    {
        var sfs = StepFactoryStore.Create();

        var hover =
            QuickInfoHelper.GetQuickInfoAsync(
                text,
                new LinePosition(line, character),
                sfs
            );

        if (!expectedHovers.Any())
            hover.MarkdownStrings.Should().BeEmpty();

        else
        {
            hover.MarkdownStrings.Should().NotBeEmpty($"Should have Hover");

            var actualHovers = hover.MarkdownStrings;

            actualHovers.Should().BeEquivalentTo(expectedHovers);
        }
    }
}
