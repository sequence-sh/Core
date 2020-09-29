using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Version = System.Version;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Allows custom serialization of versions
    /// </summary>
    public class VersionTypeConverter : IYamlTypeConverter
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