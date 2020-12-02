using System.IO;
using System.Text;

namespace Reductech.EDR.Core.Parser
{
    /// <summary>
    /// A stream of data representing a string.
    /// </summary>
    public class DataStream
    {
        /// <summary>
        /// Create a new DataStream
        /// </summary>
        public DataStream(Stream stream) => Stream = stream;

        /// <summary>
        /// The stream
        /// </summary>
        public Stream Stream { get;  }

        //TODO store encoding

        //TODO OneOf<Stream, String>

        public string Serialize()
        {
            Stream.Position = 0;
            using StreamReader reader = new StreamReader(Stream, Encoding.UTF8);
            var s = reader.ReadToEnd();

            return s;
        }
    }
}