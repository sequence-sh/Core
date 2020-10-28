using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

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


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext _)
        {
            Type elementType = Value.GetType();
            Type stepType = typeof(Constant<>).MakeGenericType(elementType);
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
                var r = SerializationMethods.TrySerializeConstant(this);

                return r.IsSuccess ? r.Value : r.Error;
            }
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