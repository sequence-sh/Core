using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;
using Thinktecture.IO;
using Thinktecture.IO.Adapters;

namespace Reductech.EDR.Core
{

/// <summary>
/// A stream of data representing a string.
/// This can either be a raw string or a stream and an encoding.
/// </summary>
public sealed class StringStream : IEquatable<StringStream>, IComparable<StringStream>, IDisposable,
                                   IComparable
{
    /// <summary>
    /// Create a new DataStream
    /// </summary>
    public StringStream(IStream stream, EncodingEnum encoding) =>
        Value = new StringStreamData.StreamData(stream, encoding);

    /// <summary>
    /// Create a new DataStream
    /// </summary>
    public StringStream(Stream stream, EncodingEnum encoding) =>
        Value = new StringStreamData.StreamData(new StreamAdapter(stream), encoding);

    /// <summary>
    /// Create a new DataStream from a string
    /// </summary>
    public StringStream(string s) => Value = new StringStreamData.ConstantData(s);

    /// <summary>
    /// Empty StringStream
    /// </summary>
    public static StringStream Empty { get; } = new("");

    /// <summary>
    /// The Value of this
    /// </summary>
    private StringStreamData Value { get; set; }

    private abstract record StringStreamData : IDisposable
    {
        public record ConstantData(string Underlying) : StringStreamData
        {
            /// <inheritdoc />
            public override async ValueTask<string> GetStringAsync()
            {
                await Task.CompletedTask;
                return Underlying;
            }

            /// <inheritdoc />
            public override string GetString()
            {
                return Underlying;
            }

            /// <inheritdoc />
            public override string Name => Underlying;

            /// <inheritdoc />
            public override string NameInLogs(bool reveal)
            {
                return
                    reveal
                        ? SerializationMethods.DoubleQuote(Underlying)
                        : $"string Length: {Underlying.Length}";
            }

            /// <inheritdoc />
            public override (IStream stream, EncodingEnum encodingEnum) GetStream()
            {
                byte[]       byteArray = Encoding.UTF8.GetBytes(Underlying);
                MemoryStream stream    = new(byteArray);

                return (new StreamAdapter(stream), EncodingEnum.UTF8);
            }

            /// <inheritdoc />
            public override void Dispose() { }
        }

        public record StreamData(IStream Stream, EncodingEnum Encoding) : StringStreamData
        {
            /// <inheritdoc />
            public override async ValueTask<string> GetStringAsync()
            {
                var stream = Stream;

                if (stream.CanSeek)
                    stream.Position = 0;

                if (stream is FakeFileStreamAdapter fake) //This is a hack
                    stream = new StreamAdapter(fake.Stream);

                using IStreamReader reader = new StreamReaderAdapter(
                    stream,
                    Encoding.Convert(),
                    true,
                    -1,
                    false
                );

                var s = await reader.ReadToEndAsync();

                return s;
            }

            /// <inheritdoc />
            public override string GetString()
            {
                Stream.Position = 0;

                using var reader = new StreamReaderAdapter(
                    Stream,
                    Encoding.Convert(),
                    true,
                    -1,
                    false
                );

                var s = reader.ReadToEnd();
                return s;
            }

            /// <inheritdoc />
            public override string Name => "StringStream";

            /// <inheritdoc />
            public override string NameInLogs(bool reveal)
            {
                return Encoding.GetDisplayName() + "-Stream";
            }

            /// <inheritdoc />
            public override void Dispose()
            {
                Stream.Dispose();
            }

            /// <inheritdoc />
            public override (IStream stream, EncodingEnum encodingEnum) GetStream()
            {
                return (Stream, Encoding);
            }
        }

        public abstract (IStream stream, EncodingEnum encodingEnum) GetStream();
        public abstract ValueTask<string> GetStringAsync();
        public abstract string GetString();

        public abstract string Name { get; }
        public abstract string NameInLogs(bool reveal);

        /// <inheritdoc />
        public abstract void Dispose();
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary>
    /// How this stringStream will appear in the logs.
    /// </summary>
    public string NameInLogs(bool reveal) => Value.NameInLogs(reveal);

    /// <summary>
    /// If this is a string, return the string, otherwise read the stream as a string.
    /// </summary>
    public async Task<string> GetStringAsync()
    {
        if (Value is StringStreamData.ConstantData cd)
            return cd.Underlying;

        await _semaphore.WaitAsync();

        try
        {
            var s = await Value.GetStringAsync();

            Value = new StringStreamData.ConstantData(s);
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
        if (Value is StringStreamData.ConstantData cd)
            return cd.Underlying;

        _semaphore.Wait();

        try
        {
            var s = Value.GetString();

            Value = new StringStreamData.ConstantData(s);

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
    public (IStream stream, EncodingEnum encodingEnum) GetStream()
    {
        _semaphore.Wait();

        try
        {
            return Value.GetStream();
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
        var s    = GetString();
        var text = SerializationMethods.DoubleQuote(s);

        return text;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is StringStream ss)
            return CompareTo(ss);

        return 0;
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
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetString().Equals(other.GetString());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

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

        Value.Dispose();
    }
}

}
