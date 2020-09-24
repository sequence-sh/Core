using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Processes.Serialization
{
    internal sealed class GeneralDeserializer : INodeDeserializer
    {
        public GeneralDeserializer(IReadOnlyList<ITypedYamlDeserializer> deserializers) => Deserializers = deserializers;

        public IReadOnlyList<ITypedYamlDeserializer> Deserializers { get; }

        /// <inheritdoc />
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            foreach (var typedYamlDeserializer in Deserializers)
            {
                if (!expectedType.IsAssignableFrom(typedYamlDeserializer.Type)) continue;

                var (isSuccess, _, o, yamlException) = typedYamlDeserializer.TryDeserializeObject(reader, nestedObjectDeserializer);

                if (isSuccess)
                {
                    value = o;
                    return true;
                }

                throw yamlException;
            }

            value = null;
            return false;
        }
    }
}