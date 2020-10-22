using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a fixed value when run.
    /// </summary>
    public sealed class ConstantFreezableStep : IFreezableStep, IEquatable<ConstantFreezableStep>
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
                if (Value is string s)
                    return $"'{s}'";

                if (Value is Enum e)
                    return e.GetDisplayName();

                return $"{Value}";
            }
        }

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => new ActualTypeReference(Value.GetType());

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(ConstantFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value.Equals(other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ConstantFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>
        /// Equals operator.
        /// </summary>
        public static bool operator ==(ConstantFreezableStep? left, ConstantFreezableStep? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(ConstantFreezableStep? left, ConstantFreezableStep? right) => !Equals(left, right);
    }
}