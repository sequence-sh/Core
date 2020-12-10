using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Serialization;
using Option = OneOf.OneOf<string, (System.IO.Stream, Reductech.EDR.Core.EncodingEnum)>;

namespace Reductech.EDR.Core.Parser
{
    /// <summary>
    /// A stream of data representing a string.
    /// This can either be a raw string or a stream and an encoding.
    /// </summary>
    public class StringStream : IEquatable<StringStream>, IComparable<StringStream>
    {
        /// <summary>
        /// Create a new DataStream
        /// </summary>
        public StringStream(Stream stream, EncodingEnum encoding) => Value = (stream, encoding);

        /// <summary>
        /// Create a new DataStream from a string
        /// </summary>
        public StringStream(string s) => Value = s;

        /// <summary>
        /// If this is a string, the string.
        /// If this is a stream, "StringStream"
        /// </summary>
        public string Name
        {
            get
            {
                return Value.Match(x => x, x => "StringStream");
            }
        }


        /// <summary>
        /// If this is a string, return the string, otherwise read the stream as a string.
        /// </summary>
        public async Task<string> GetStringAsync()
        {
            var result = Value.Match(Task.FromResult, SerializeStreamAsync);

            return await result;


            static async Task<string> SerializeStreamAsync((Stream stream, EncodingEnum encodingEnum) streamPair)
            {
                var stream = streamPair.stream;
                var encodingEnum = streamPair.encodingEnum;

                stream.Position = 0;
                using StreamReader reader = new StreamReader(stream, encodingEnum.Convert(), leaveOpen: true);
                var s = await reader.ReadToEndAsync();

                return s;
            }
        }

        /// <summary>
        /// If this is a string, return the string, otherwise read the stream as a string.
        /// </summary>
        public string GetString()
        {
            var result = Value.Match(x=>x, SerializeStream);

            return result;


            static string SerializeStream((Stream stream, EncodingEnum encodingEnum) streamPair)
            {
                var stream = streamPair.stream;
                var encodingEnum = streamPair.encodingEnum;

                stream.Position = 0;
                using StreamReader reader = new StreamReader(stream, encodingEnum.Convert(), leaveOpen: true);

                var s = reader.ReadToEnd();

                return s;
            }
        }


        /// <summary>
        /// Implicit operator converting strings to StringStreams
        /// </summary>
        public static implicit operator StringStream(string str)
        {
            StringStream stringStream = new StringStream(str);
            return stringStream;
        }

        /// <summary>
        /// If this is a stream, return it. Otherwise, return the string as a stream.
        /// </summary>
        /// <returns></returns>
        public (Stream stream, EncodingEnum encodingEnum) GetStream()
        {
            return Value.Match(x =>
            {
                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(x);
                MemoryStream stream = new MemoryStream(byteArray);

                return (stream, EncodingEnum.UTF8);

            }, x => x);
        }

        /// <summary>
        /// The Value of this
        /// </summary>
        public Option Value { get; }

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <summary>
        /// SerializeAsync this DataStream
        /// </summary>
        public async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {

            var s = await GetStringAsync();

            var text = SerializationMethods.DoubleQuote(s);

            return text;
        }

        /// <inheritdoc />
        public bool Equals(StringStream? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetString().Equals(other.GetString());
        }

        /// <inheritdoc />
        public int CompareTo(StringStream? other)
        {
            if (other is null)
                return 1.CompareTo(null);


            return StringComparer.Ordinal.Compare(GetString(), other.GetString());

        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is StringStream ss && Equals(ss);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Value.Match(x => x.GetHashCode(), x => x.GetHashCode());

        /// <summary>
        /// Equals operator.
        /// </summary>
        public static bool operator ==(StringStream? left, StringStream? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(StringStream? left, StringStream? right) => !Equals(left, right);
    }
}