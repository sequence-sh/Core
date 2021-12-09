using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization;

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
    public static string Serialize(this Entity entity)
    {
        var sb = new StringBuilder();

        sb.Append('(');

        var results = new List<string>();

        foreach (var property in entity)
            results.Add($"'{property.Name}': {property.Value.Serialize()}");

        sb.AppendJoin(" ", results);

        sb.Append(')');

        var result = sb.ToString();

        return result;
    }

    /// <summary>
    /// Format this entity as a multiline indented string
    /// </summary>
    public static string Format(this Entity entity)
    {
        var sb = new StringBuilder();
        FormatEntity(entity, sb, 0, null, null);
        return sb.ToString();

        static void FormatEntityValue(
            StringBuilder sb,
            int indentation,
            EntityValue ev,
            string? prefix,
            string? suffix)
        {
            if (ev is EntityValue.NestedEntity nestedEntity)
            {
                FormatEntity(
                    nestedEntity.Value,
                    sb,
                    indentation,
                    prefix,
                    suffix
                );
            }
            else if (ev is EntityValue.NestedList nestedList)
            {
                if (nestedList.Value.Any(
                        x => x is EntityValue.NestedEntity || x is EntityValue.NestedList
                    ))
                {
                    AppendLineIndented(
                        sb,
                        indentation,
                        prefix + "["
                    );

                    indentation++;

                    for (var index = 0; index < nestedList.Value.Count; index++)
                    {
                        var entityValue = nestedList.Value[index];
                        var maybeComma  = index < nestedList.Value.Count - 1 ? "," : null;

                        FormatEntityValue(
                            sb,
                            indentation,
                            entityValue,
                            null,
                            maybeComma
                        );
                    }

                    indentation--;
                    AppendLineIndented(sb, indentation, "]" + suffix);
                }
                else
                {
                    var line = "[" + string.Join(", ", nestedList.Value.Select(x => x.Serialize()))
                                   + "]";

                    AppendLineIndented(
                        sb,
                        indentation,
                        prefix + line + suffix
                    );
                }
            }
            else
            {
                AppendLineIndented(
                    sb,
                    indentation,
                    prefix + ev.Serialize() + suffix
                );
            }
        }

        static void FormatEntity(
            Entity entity,
            StringBuilder sb,
            int indentation,
            string? prefix,
            string? suffix)
        {
            AppendLineIndented(sb, indentation, prefix + "(");

            indentation++;

            foreach (var property in entity)
            {
                FormatEntityValue(
                    sb,
                    indentation,
                    property.Value,
                    $"'{property.Name}': ",
                    null
                );
            }

            indentation--;
            AppendLineIndented(sb, indentation, ")" + suffix);
        }

        static void AppendLineIndented(StringBuilder sb, int indentation, string value)
        {
            sb.Append('\t', indentation);
            sb.AppendLine(value);
        }
    }

    /// <summary>
    /// Converts an object to a string suitable for printing
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
                SCLNull         => "Null",
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
                SCLNull         => "Null",
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
