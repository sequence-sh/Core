using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Core.Serialization
{
    internal sealed class GeneralDeserializer : INodeDeserializer
    {
        public GeneralDeserializer(IReadOnlyList<ITypedYamlDeserializer> deserializers) => Deserializers = deserializers;

        public IReadOnlyList<ITypedYamlDeserializer> Deserializers { get; }

        /// <inheritdoc />
        public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (reader.Current == null)
            {
                value = null;
                return false;
            }

            foreach (var typedYamlDeserializer in Deserializers)
            {
                if (!expectedType.IsAssignableFrom(typedYamlDeserializer.Type)) continue;

                var (isSuccess, _, o, error) = typedYamlDeserializer.TryDeserializeObject(reader, nestedObjectDeserializer);

                if (isSuccess)
                {
                    value = o;
                    return true;
                }


                throw new GeneralSerializerYamlException(reader.Current.Start, reader.Current.End, error);
            }

            value = null;
            return false;
        }
    }


#pragma warning disable CA1032 // Implement standard exception constructors
    internal class GeneralSerializerYamlException : YamlException
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <inheritdoc />
        public GeneralSerializerYamlException([NotNull] Mark start, [NotNull] Mark end, [NotNull] IError error) : base(start, end, error.AsString)
        {
            Error = error;
        }

        public IError Error { get; }
    }

    internal static class YamlExceptionHelper
    {
        public static string GetRealMessage(this YamlException exception)
        {
            var newMessage = exception.Message.Split(exception.End.ToString()).Last()
                .TrimStart(')', ':', ' ');

            return newMessage;
        }
    }
}