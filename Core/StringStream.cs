using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;
using Option = OneOf.OneOf<string, (System.IO.Stream, Reductech.EDR.Core.Enums.EncodingEnum)>;

namespace Reductech.EDR.Core
{
    /// <summary>
    /// A stream of data representing a string.
    /// This can either be a raw string or a stream and an encoding.
    /// </summary>
    public sealed class StringStream : IEquatable<StringStream>, IComparable<StringStream>, IDisposable
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
        /// The Value of this
        /// </summary>
        public Option Value { get; private set; }

        /// <inheritdoc />
        public override string ToString() => Name;

        private readonly SemaphoreSlim _semaphore = new(1);


        /// <summary>
        /// If this is a string, the string.
        /// If this is a stream, "StringStream"
        /// </summary>
        public string Name => Value.Match(x => x, _ => "StringStream");

        /// <summary>
        /// How this stringStream will appear in the logs.
        /// </summary>
        public string NameInLogs(bool reveal) =>
            Value.TryPickT0(out var text, out var remainder)
                ? reveal? Serialize() //TODO truncate
                : $"string Length: {text.Length}"
                : remainder.Item2.GetDisplayName() + "-Stream";

        /// <summary>
        /// If this is a string, return the string, otherwise read the stream as a string.
        /// </summary>
        public async Task<string> GetStringAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                if (Value.IsT0)
                    return Value.AsT0;

                var (stream, encodingEnum) = Value.AsT1;

                stream.Position = 0;
                using StreamReader reader = new(stream, encodingEnum.Convert(), leaveOpen: false);
                var s = await reader.ReadToEndAsync();

                Value = s;

                return s;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// If this is a string, return the string, otherwise read the stream as a string.
        /// </summary>
        public string GetString()
        {
            _semaphore.Wait();

            try
            {
                if (Value.IsT0)
                    return Value.AsT0;

                var (stream, encodingEnum) = Value.AsT1;

                stream.Position = 0;
                using StreamReader reader = new(stream, encodingEnum.Convert(), leaveOpen: false);
                var s = reader.ReadToEnd();

                Value = s;

                return s;
            }
            finally
            {
                _semaphore.Release();
            }
        }


        /// <summary>
        /// Implicit operator converting strings to StringStreams
        /// </summary>
        public static implicit operator StringStream(string str)
        {
            StringStream stringStream = new(str);
            return stringStream;
        }

        /// <summary>
        /// If this is a stream, return it.
        /// Otherwise, return the string as a stream.
        /// You should dispose of this stream after using it.
        /// </summary>
        /// <returns></returns>
        public (Stream stream, EncodingEnum encodingEnum) GetStream()
        {
            _semaphore.Wait();

            try
            {
                return Value.Match(x =>
            {
                // convert string to stream
                byte[] byteArray = Encoding.UTF8.GetBytes(x);
                MemoryStream stream = new(byteArray);

                return (stream, EncodingEnum.UTF8);

            }, x => x);
            }
            finally
            {
                _semaphore.Release();
            }
        }



        /// <summary>
        /// SerializeAsync this DataStream
        /// </summary>
        public string Serialize()
        {
            var s = GetString();
            var text = SerializationMethods.DoubleQuote(s);

            return text;
        }

        /// <inheritdoc />
        public int CompareTo(StringStream? other)
        {
            if (other is null)
                return 1.CompareTo(null);


            return StringComparer.Ordinal.Compare(GetString(), other.GetString());

        }

        /// <inheritdoc />
        public bool Equals(StringStream? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetString().Equals(other.GetString());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;

            return obj is StringStream ss && Equals(ss);
        }

        /// <inheritdoc />
        public override int GetHashCode() => GetString().GetHashCode();

        /// <summary>
        /// Equals operator.
        /// </summary>
        public static bool operator ==(StringStream? left, StringStream? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(StringStream? left, StringStream? right) => !Equals(left, right);

        /// <inheritdoc />
        public void Dispose()
        {
            _semaphore.Dispose();
            if(Value.IsT1)
                Value.AsT1.Item1.Dispose();
        }
    }
}