using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Version = System.Version;

namespace Reductech.EDR.Core.Serialization
{

    internal class YamlString
    {
        public YamlString(string value) { Value = value; }

        public string Value { get; }

        /// <inheritdoc />
        public override string ToString() => Value;
    }

    /// <summary>
    /// Allows custom serialization of strings.
    /// Makes sure the string is serialized with quotes.
    /// </summary>
    internal class YamlStringTypeConverter : IYamlTypeConverter
    {
        private YamlStringTypeConverter() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static IYamlTypeConverter Instance { get; } = new YamlStringTypeConverter();

        /// <inheritdoc />
        public bool Accepts(Type type) => type == typeof(YamlString);

        /// <inheritdoc />
        public object? ReadYaml(IParser parser, Type type) => null;

        /// <inheritdoc />
        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if(value is YamlString s)
                emitter.Emit(new Scalar(null, null, s.Value, ScalarStyle.DoubleQuoted, true, false));
        }
    }

    /// <summary>
    /// Allows custom serialization of versions
    /// </summary>
    internal class VersionTypeConverter : IYamlTypeConverter
    {
        private VersionTypeConverter() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IYamlTypeConverter Instance { get; } = new VersionTypeConverter();

        /// <inheritdoc />
        public bool Accepts(Type type) => type == typeof(Version);

        /// <inheritdoc />
        public object? ReadYaml(IParser parser, Type type)
        {
            if (type != typeof(Version) || !parser.TryConsume<Scalar>(out var scalar)) return null;

            var version = new Version(scalar.Value);
            return version;

        }

        /// <inheritdoc />
        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value is Version version) emitter.Emit(new Scalar(version.ToString()));
        }
    }
}