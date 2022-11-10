using System.Text.RegularExpressions;

namespace Sequence.Core.TestHarness;

public static class SpaceCompressor
{
    /// <summary>
    /// Regex for blank spaces
    /// </summary>
    private static readonly Regex SpaceRegex =
        new(@"(?:\s+|\r\n|\n|\t|\\r\\n|\\n|\\t)", RegexOptions.Compiled);

    private static readonly Regex NewLineRegex = new(@"(?:\r\n|\n)", RegexOptions.Compiled);

    /// <summary>
    /// Compresses whitespace characters AND escaped whitespace characters
    /// </summary>
    public static string CompressSpaces(string text) => SpaceRegex.Replace(text, " ");

    public static string CompressNewLines(string text) => NewLineRegex.Replace(text, " ");
}
