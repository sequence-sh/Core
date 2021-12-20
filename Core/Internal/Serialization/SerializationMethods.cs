using System.Text;

namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Serializes primitive types
/// </summary>
public static class SerializationMethods
{
    /// <summary>
    /// SerializeAsync a list
    /// </summary>
    public static string SerializeList(IEnumerable<string> serializedMembers)
    {
        var sb2 = new StringBuilder();

        sb2.Append('[');
        sb2.AppendJoin(", ", serializedMembers);
        sb2.Append(']');

        return sb2.ToString();
    }

    /// <summary>
    /// Quote with single quotes
    /// </summary>
    public static string SingleQuote(string s) => "'" + s.Replace("'", "''") + "'";

    /// <summary>
    /// Quote with double quotes
    /// </summary>
    public static string DoubleQuote(string s)
    {
        var result = "\"" + Escape(s) + "\"";

        return result;
    }

    /// <summary>
    /// Escapes a string for double quotes
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string Escape(string s)
    {
        var result = s
                .Replace("\\", @"\\") //Needs to come first
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\"", @"\""")
            ;

        return result;
    }
}
