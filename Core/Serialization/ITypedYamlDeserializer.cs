using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;

namespace Reductech.EDR.Core.Serialization
{
    internal interface ITypedYamlDeserializer
    {
        Type Type { get; }

        public Result<object, IError> TryDeserializeObject(IParser reader,
            Func<IParser, Type, object?> nestedObjectDeserializer);
    }
}