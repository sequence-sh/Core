using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Reductech.EDR.Core.Serialization
{
    internal abstract class TypedYamlDeserializer<T> : ITypedYamlDeserializer
    {
        public abstract Result<T, IError> TryDeserialize(IParser reader,
            Func<IParser, Type, object?> nestedObjectDeserializer);

        /// <inheritdoc />
        public Type Type => typeof(T);

        /// <inheritdoc />
        public Result<object, IError> TryDeserializeObject(IParser reader, Func<IParser, Type, object?> nestedObjectDeserializer) =>
            TryDeserialize(reader, nestedObjectDeserializer).Map(x=> (x as object)!);

        protected static Result<T2, IError> TryDeserializeNested<T2>(Func<IParser, Type, object?> nestedObjectDeserializer, IParser reader)
        {
            object? r;

            try
            {
                r = nestedObjectDeserializer.Invoke(reader, typeof(T2));
            }
            catch (GeneralSerializerYamlException e)
            {
                return Result.Failure<T2, IError>(e.Error);
            }
            catch (YamlException e)
            {
                return new SingleError(e.GetRealMessage(), ErrorCode.CouldNotParse, new YamlRegionErrorLocation(e.Start, e.End));
            }

            var r2 = (T2) r!;

            return r2;
        }
    }
}