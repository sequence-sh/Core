using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using DateTime = System.DateTime;
using Option = OneOf.OneOf<string, int, double, bool,
Reductech.EDR.Core.Internal.Enumeration, System.DateTime,
Reductech.EDR.Core.Entity,
Reductech.EDR.Core.Entities.EntityStream, Reductech.EDR.Core.Parser.DataStream>;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public sealed class ConstantFreezableStep : IFreezableStep
    {
        /// <summary>
        /// Creates a new ConstantFreezableStep.
        /// </summary>
        /// <param name="value"></param>
        public ConstantFreezableStep(Option value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public Option Value { get; }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {
            var elementType = TryGetType(stepContext.TypeResolver.StepFactoryStore);
            if (elementType.IsFailure) return elementType.ConvertFailure<IStep>();

            Type stepType = typeof(Constant<>).MakeGenericType(elementType.Value);
            var value = GetValue(stepContext.TypeResolver.StepFactoryStore);
            if (value.IsFailure) return value.ConvertFailure<IStep>();

            var stepAsObject = Activator.CreateInstance(stepType, value.Value);

            //TODO check for exceptions here?

            Result<IStep, IError> result;

            try
            {
                var step = (IStep)stepAsObject!;
                result = Result.Success<IStep, IError>(step);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                result = Result.Failure<IStep, IError>(new SingleError(e, ErrorCode.InvalidCast, new FreezableStepErrorLocation(this)));
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return result;
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            return Result.Success<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError>(new List<(VariableName variableName, Maybe<ITypeReference>)>());
        }

        /// <inheritdoc />
        public string StepName
        {
            get
            {
                var r = WriteValue(Value);
                return r;
            }
        }

        private Result<object, IError> GetValue(StepFactoryStore stepFactoryStore)
        {
            return Value.Match(
                Result.Success<object, IError>,
                x=>Result.Success<object, IError>(x),
                x=>Result.Success<object, IError>(x),
                x=>Result.Success<object, IError>(x),
                TryGetEnumerationValue,
                x=>Result.Success<object, IError>(x),
                Result.Success<object, IError>,
                Result.Success<object, IError>,
                Result.Success<object, IError>);

            Result<object, IError> TryGetEnumerationValue(Enumeration enumeration)
            {
                var type = TryGetType(stepFactoryStore);
                if (type.IsFailure) return type.ConvertFailure<object>();

                if (Enum.TryParse(type.Value, enumeration.Value, false, out var o))
                    return o!;

                return new SingleError($"Enum '{enumeration}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
            }
        }

        /// <summary>
        /// SerializeAsync a value.
        /// </summary>
        public static string WriteValue(Option value)
        {
            return

                value.Match(
                    SerializationMethods.DoubleQuote,
                    i => i.ToString(),
                    d => d.ToString("G17"),
                    b => b.ToString(),
                    e => e.ToString(),
                    dt => dt.ToString("O"),
                    entity => entity.Serialize(),
                    es => "EntityStream",
                    ds => ds.Serialize()//TODO change to DataStream
                );
        }


        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => TryGetType(typeResolver.StepFactoryStore)
            .Map(x => new ActualTypeReference(x) as ITypeReference);

        private Result<Type, IError> TryGetType(StepFactoryStore stepFactoryStore)
        {
            var type = Value.Match(
               _ => typeof(string),
               _ => typeof(int),
               _ => typeof(double),
               _ => typeof(bool),
               GetEnumerationType,
               _ => typeof(DateTime),
               _ => typeof(Entity),
               _ => typeof(EntityStream),
               _ => typeof(DataStream));

            return type;

            Result<Type, IError> GetEnumerationType(Enumeration enumeration)
            {
                if (stepFactoryStore.EnumTypesDictionary.TryGetValue(enumeration.Type, out var t))
                    return t;
                return new SingleError($"Enum '{enumeration.Type}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
            }
        }

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other is ConstantFreezableStep cfs && Value.Equals(cfs.Value);
        }


        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();
    }
}