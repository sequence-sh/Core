using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

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

    /// <summary>
    /// Serialize this entity.
    /// </summary>
    /// <returns></returns>
    public static string Serialize(this Entity entity)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var property in entity)
            results.Add($"{property.Name}: {property.BestValue.Serialize()}");

        sb.AppendJoin(" ", results);

        sb.Append(')');

        var result = sb.ToString();

        return result;
    }

    /// <summary>
    /// Converts an object to a string suitable from printing
    /// </summary>
    public static string GetString(object? obj)
    {
        return
            obj switch
            {
                Entity entity   => entity.Serialize(),
                StringStream ss => ss.GetString(),
                DateTime dt     => dt.ToString(Constants.DateTimeFormat),
                double d        => d.ToString(Constants.DoubleFormat, new NumberFormatInfo()),
                IArray array    => array.NameInLogs,
                _               => obj?.ToString()!
            };
    }

    /// <summary>
    /// Serialize any object
    /// </summary>
    public static string SerializeObject(object? obj)
    {
        return
            obj switch
            {
                Entity entity   => entity.Serialize(),
                StringStream ss => ss.Serialize(),
                DateTime dt     => dt.ToString(Constants.DateTimeFormat),
                double d        => d.ToString(Constants.DoubleFormat, new NumberFormatInfo()),
                Enumeration enu => enu.Serialize,
                IArray array    => array.Serialize,
                _               => obj?.ToString()!
            };
    }

    /// <summary>
    /// Converts an object to a string suitable from printing
    /// </summary>
    public static async Task<string> GetStringAsync(object? obj)
    {
        return
            obj switch
            {
                StringStream ss => await ss.GetStringAsync(),
                _               => GetString(obj)
            };
    }
}

}
