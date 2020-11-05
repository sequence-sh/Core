using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entities.Entity;

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
        public ConstantFreezableStep(object value) => Value = value;

        /// <summary>
        /// The value that this will return when run.
        /// </summary>
        public object Value { get; }

        private Type ElementType
        {
            get
            {
                if (Value is Stream) return typeof(Stream); //We always need to return the base type for stream.s
                var type = Value.GetType();
                return type;
            }
        }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext _)
        {
            Type stepType = typeof(Constant<>).MakeGenericType(ElementType);
            var stepAsObject = Activator.CreateInstance(stepType, Value);

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
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(TypeResolver typeResolver) =>
            ImmutableList<(VariableName VariableName, ITypeReference type)>.Empty;

        /// <inheritdoc />
        public string StepName
        {
            get
            {
                var r = WriteValue(Value, false);
                return r;
            }
        }

        /// <summary>
        /// Serialize a value.
        /// </summary>
        public static string WriteValue(object value, bool prefixEnumNames)
        {
            if (value is string s)
                return $"'{s}'";

            if (value is IEnumerable enumerable)
            {

                var r = string.Join(", ", enumerable.OfType<object>().Select(x => WriteValue(x, prefixEnumNames)));

                return $"[{r}]";
            }

            if (!prefixEnumNames && value is Enum e)
                return e.GetDisplayName();


            if (value is Stream)
            {
                return "Stream";
                //return SerializationMethods.StreamToString(stream, Encoding.UTF8);
            }

            if (value is Entity entity)
            {
                var r = entity.TrySerializeShortForm();
                if (r.IsSuccess) return r.Value;
                return "Entity";
            }

            if (value is EntityStream)
            {
                return "EntityStream";
            }

            var simpleResult = SerializationMethods.TrySerializeSimple(value);

            if(simpleResult.IsFailure)
                throw new SerializationException(simpleResult.Value);

            return simpleResult.Value;
        }



        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => new ActualTypeReference(Value.GetType());

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