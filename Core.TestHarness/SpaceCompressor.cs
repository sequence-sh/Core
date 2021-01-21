using System.Text.RegularExpressions;

namespace Reductech.EDR.Core.TestHarness
{

public static class SpaceCompressor
{
    /// <summary>
    /// Regex for blank spaces
    /// </summary>
    private static readonly Regex SpaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string CompressSpaces(string stepName) => SpaceRegex.Replace(stepName, " ");
}

}
