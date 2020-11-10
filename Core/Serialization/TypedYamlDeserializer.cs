using System;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using YamlDotNet.Core;

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

        protected static Result<object, IError> TryDeserializeNested(Func<IParser, Type, object?> nestedObjectDeserializer, Type type, IParser reader)
        {
            object? r;

            try
            {
                r = nestedObjectDeserializer.Invoke(reader, type);
            }
            catch (GeneralSerializerYamlException e)
            {
                return Result.Failure<object, IError>(e.Error);
            }
            catch (YamlException e)
            {
                return new SingleError(e.GetRealMessage(), ErrorCode.CouldNotParse, new YamlRegionErrorLocation(e.Start, e.End));
            }


            return r!;
        }

        protected static Result<T2, IError> TryDeserializeNested<T2>(Func<IParser, Type, object?> nestedObjectDeserializer, IParser reader)
        {
            var result = TryDeserializeNested(nestedObjectDeserializer, typeof(T2), reader);

            if(result.IsFailure)
                return result.ConvertFailure<T2>();

            var r2 = (T2)result.Value;

            return r2!;
        }
    }
}