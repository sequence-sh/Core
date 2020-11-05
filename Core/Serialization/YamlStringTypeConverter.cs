using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Core.Serialization
{
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
                emitter.Emit(new Scalar(null, null, s.Value, ScalarStyle.SingleQuoted, true, false));
        }
    }
}