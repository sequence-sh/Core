using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        /// Serialize an entityStream
        /// </summary>
        public static async Task<string> SerializeEntityStreamAsync(EntityStream entityStream, CancellationToken cancellationToken)
        {
            var enumerable = await entityStream.SourceEnumerable.Select(x=>x.Serialize()) .ToListAsync(cancellationToken);

            return SerializeList(enumerable);
        }
    }
}
