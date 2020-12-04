using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;

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
        public DataStream(Stream stream, EncodingEnum encoding)
        {
            Stream = stream;
            EncodingEnum = encoding;
        }

        /// <summary>
        /// Create a new DataStream from a string
        /// </summary>
        public DataStream(string s)
        {
            // convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            Stream = new MemoryStream(byteArray);
            EncodingEnum = EncodingEnum.UTF8;
        }

        /// <summary>
        /// The stream
        /// </summary>
        public Stream Stream { get;  }

        /// <summary>
        /// The encoding of the stream
        /// </summary>
        public EncodingEnum EncodingEnum { get; }

        //TODO OneOf<Stream, String>
        /// <summary>
        /// SerializeAsync this DataStream
        /// </summary>
        public async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {
            Stream.Position = 0;
            using StreamReader reader = new StreamReader(Stream, Encoding.UTF8, leaveOpen: true);
            var s = await reader.ReadToEndAsync();

            var toStream = new StringToStream()
            {
                String = new Constant<string>(s),
                Encoding = new Constant<EncodingEnum>(EncodingEnum)
            };
            var r = await toStream.SerializeAsync(cancellationToken);

            return r;
        }
    }
}