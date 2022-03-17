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
    [InlineData("Print True", 0, 7, "`True`", "`SCLBool`")]
    [InlineData("Print .", 0, 7, "Syntax Error: no viable alternative at input 'Print .'")]
    [InlineData("Print 1990-01-06", 0, 7, "`1990-01-06`", "`SCLDateTime`")]
    [InlineData("Print 123", 0, 8, "`123`", "`SCLInt`")]
    [InlineData("Print 123.45", 0, 8, "`123.45`", "`SCLDouble`")]
    [InlineData("Print TextCase.Fake", 0, 8, "'Fake' is not a member of enumeration 'TextCase'")]
    [InlineData("Print Fake.Enum", 0, 8, "'Fake' is not a valid enum type.")]
    [InlineData(
        "StringToCase 'abc' TextCase.Upper",
        0,
        15,
        "`'abc'`",
        "`StringStream`"
    )]
    [InlineData(
        "StringToCase string:'abc' Case: TextCase.Upper",
        0,
        28,
        "`Case`",
        "`SCLEnum`1`",
        "The case to change to."
    )]
    [InlineData("Foreach [1,2,3] (<> => Print <>)", 0, 30, "`<>`", "Automatic Variable")]
    [InlineData(
        "print TextCase.Upper",
        0,
        8,
        "`Upper`",
        "`TextCase`",
        "The case to convert the text to."
    )]
    [InlineData("1  +  2", 0, 2, "`Sum`", "`int`", "Calculate the sum of a list of integers")]
    [InlineData("<a> = 1", 0, 5, "`SetVariable`", "`Unit`", "Sets the value of a named variable.")]
    [InlineData("Print <a>", 0, 7, "`<a>`", "`Any`")]
    [InlineData(
        "- <a> = 1\r\n- Print <a>",
        1,
        10,
        "`<a>`",
        "`SCLInt`"
    )]
    [InlineData(
        "Print [1,2,3]",
        0,
        6,
        "`ArrayNew`",
        "`Array<int>`",
        "Represents an ordered collection of objects."
    )]
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
    public void ShouldGiveCorrectQuickInfo(
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
