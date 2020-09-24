using System;
using CSharpFunctionalExtensions;
using YamlDotNet.Core;

namespace Reductech.EDR.Processes.Serialization
{
    internal abstract class TypedYamlDeserializer<T> : ITypedYamlDeserializer
    {
        public abstract Result<T, YamlException> TryDeserialize(IParser reader,
            Func<IParser, Type, object?> nestedObjectDeserializer);

        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public Result<object, YamlException> TryDeserializeObject(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer) =>
            TryDeserialize(reader, nestedObjectDeserializer).Map(x=> (x as object)!);

        protected static Result<T2, YamlException> TryDeserializeNested<T2>(Func<IParser, Type, object?> nestedObjectDeserializer, IParser reader)
        {
            object? r;

            try
            {
                r = nestedObjectDeserializer.Invoke(reader, typeof(T2));
            }
            catch (YamlException e)
            {
                return e;
            }

            var r2 = (T2) r!;

            return r2;
        }
    }
}