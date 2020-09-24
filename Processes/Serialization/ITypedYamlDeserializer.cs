using System;
using CSharpFunctionalExtensions;
using YamlDotNet.Core;

namespace Reductech.EDR.Processes.Serialization
{
    internal interface ITypedYamlDeserializer
    {
        Type Type { get; }

        public Result<object, YamlException> TryDeserializeObject(IParser reader,
            Func<IParser, Type, object?> nestedObjectDeserializer);
    }
}