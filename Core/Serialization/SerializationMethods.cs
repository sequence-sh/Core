using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reductech.EDR.Core.Entities;

namespace Reductech.EDR.Core.Serialization
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
            var result = s
                .Replace("\\", @"\\") //Needs to come first
                .Replace("\r", @"\r")
                .Replace("\n", @"\n")
                .Replace("\t", @"\t")
                .Replace("\"", @"\""")

                ;


            result = "\"" + result + "\"";

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
        /// Serialize an EntityValue
        /// </summary>
        public static string Serialize(this EntityValue entityValue)
        {
            return entityValue.Value.Match(_ => DoubleQuote(""),
                DoubleQuote,
                x => x.ToString(),
                x => x.ToString("G17"),
                x => x.ToString(),
                x => x.ToString(),
                x => x.ToString("O"),
                x => x.Serialize(),
                x => SerializeList(x.Select(y => y.Serialize()))
            );
        }
    }
}
