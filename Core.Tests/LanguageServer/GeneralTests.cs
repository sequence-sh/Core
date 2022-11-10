using Sequence.Core.LanguageServer.Objects;

namespace Sequence.Core.Tests.LanguageServer;

public class GeneralTests
{
    [Fact]
    public void Line_Positions_Should_Be_Ordered_Correctly()
    {
        var lps = new List<LinePosition>()
        {
            new(1, 0),
            new(1, 0),
            new(1, 1),
            new(2, 0),
            new(2, 1),
        };

        var reordered = lps.AsEnumerable()
            .Reverse()
            .OrderBy(x => x);

        reordered.Should().BeEquivalentTo(lps);

        var reordered2 = lps.AsEnumerable()
            .OrderByDescending(x => x)
            .Reverse();

        reordered2.Should().BeEquivalentTo(lps);

        for (var i1 = 1; i1 < lps.Count; i1++)
        {
            var linePosition1 = lps[i1];

            for (var i2 = 1; i2 < lps.Count; i2++)
            {
                var linePosition2 = lps[i2];

                if (i1 < i2)
                {
                    (linePosition1 < linePosition2).Should().BeTrue();
                    (linePosition1 <= linePosition2).Should().BeTrue();

                    (linePosition1 >= linePosition2).Should().BeFalse();
                    (linePosition1 > linePosition2).Should().BeFalse();
                }
                else if (i1 == i2)
                {
                    (linePosition1 >= linePosition2).Should().BeTrue();
                    (linePosition1 <= linePosition2).Should().BeTrue();

                    (linePosition1 > linePosition2).Should().BeFalse();
                    (linePosition1 < linePosition2).Should().BeFalse();
                }
                else
                {
                    (linePosition1 >= linePosition2).Should().BeTrue();
                    (linePosition1 > linePosition2).Should().BeTrue();

                    (linePosition1 <= linePosition2).Should().BeFalse();
                    (linePosition1 < linePosition2).Should().BeFalse();
                }
            }
        }
    }
}
