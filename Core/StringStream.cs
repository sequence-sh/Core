using System.IO;
using System.Text;
using Reductech.Sequence.Core.Enums;

namespace Reductech.Sequence.Core;

/// <summary>
/// A stream of data representing a string.
/// This can either be a raw string or a stream and an encoding.
/// </summary>
public sealed class StringStream : IEquatable<StringStream>, IComparable<StringStream>,
                                   IDisposable, IComparableSCLObject
{
    /// <summary>
    /// Create a new DataStream
    /// </summary>
    public StringStream(Stream stream, EncodingEnum encoding) =>
        Value = new StringStreamData.StreamData(stream, encoding);

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
            public override string GetString() => Underlying;

            /// <inheritdoc />
            public override string Serialize(SerializeOptions serializeOptions)
            {
                var text = GetString();

                if (serializeOptions.HideStrings)
                    return $"string Length: {text.Length}";

                if (serializeOptions.QuoteStrings)
                    text = SerializationMethods.DoubleQuote(text);

                return text;
            }

            /// <inheritdoc />
            public override (Stream stream, EncodingEnum encodingEnum) GetStream()
            {
                byte[]       byteArray = Encoding.UTF8.GetBytes(Underlying);
                MemoryStream stream    = new(byteArray);

                return (stream, EncodingEnum.UTF8);
            }

            /// <inheritdoc />
            public override void Dispose() { }
        }

        public record StreamData(Stream Stream, EncodingEnum Encoding) : StringStreamData
        {
            /// <inheritdoc />
            public override async ValueTask<string> GetStringAsync()
            {
                var stream = Stream;

                if (stream.CanSeek)
                    stream.Position = 0;

                using StreamReader reader = new(
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

                using var reader = new StreamReader(
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
            public override string Serialize(SerializeOptions serializeOptions)
            {
                if (!serializeOptions.EvaluateStreams)
                    return Encoding.GetDisplayName() + "-Stream";

                var text = GetString();

                if (serializeOptions.HideStrings)
                    return $"string Length: {text.Length}";

                if (serializeOptions.QuoteStrings)
                    text = SerializationMethods.DoubleQuote(text);

                return text;
            }

            /// <inheritdoc />
            public override void Dispose()
            {
                Stream.Dispose();
            }

            /// <inheritdoc />
            public override (Stream stream, EncodingEnum encodingEnum) GetStream()
            {
                return (Stream, Encoding);
            }
        }

        public abstract (Stream stream, EncodingEnum encodingEnum) GetStream();
        public abstract ValueTask<string> GetStringAsync();

        /// <summary>
        /// The the verbatim string
        /// </summary>
        public abstract string GetString();

        /// <summary>
        /// Serialize according to the options
        /// </summary>
        public abstract string Serialize(SerializeOptions serializeOptions);

        /// <inheritdoc />
        public abstract void Dispose();
    }

    /// <inheritdoc />
    public override string ToString() => GetString();

    private readonly SemaphoreSlim _semaphore = new(1);

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
    public (Stream stream, EncodingEnum encodingEnum) GetStream()
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
    public string Serialize(SerializeOptions options) => Value.Serialize(options);

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

    /// <inheritdoc />
    public TypeReference GetTypeReference() => TypeReference.Actual.String;

    /// <inheritdoc />
    public object ToCSharpObject() => GetString();

    /// <inheritdoc />
    public Maybe<T> MaybeAs<T>() where T : ISCLObject
    {
        if (this is T value)
            return value;

        if (typeof(T).IsGenericType && typeof(T).GetInterfaces().Contains(typeof(ISCLEnum)))
        {
            var enumType = typeof(T).GenericTypeArguments.Single();

            if (Enum.TryParse(enumType, GetString(), true, out var r))
            {
                var instance = Activator.CreateInstance(typeof(T), r);
                var result   = (T)instance!;
                return result;
            }
        }

        return Maybe<T>.None;
    }

    /// <inheritdoc />
    public SchemaNode ToSchemaNode(
        string path,
        SchemaConversionOptions? schemaConversionOptions)
    {
        if (schemaConversionOptions is null)
            return StringNode.Default;

        return schemaConversionOptions.GetNode(this.GetString(), path);
    }
}
