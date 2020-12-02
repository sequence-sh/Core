using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using DateTime = System.DateTime;
using Entity = Reductech.EDR.Core.Entities.Entity;
using Option = OneOf.OneOf<string, int, double, bool,
Reductech.EDR.Core.Internal.Enumeration, System.DateTime,
Reductech.EDR.Core.Entities.Entity,
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

        private Type ElementType
        {
            get
            {
                return Value.Match(
                    _ => typeof(string),
                    _ => typeof(int),
                    _ => typeof(double),
                    _ => typeof(bool),
                    _ => typeof(Enumeration),
                    _ => typeof(DateTime),
                    _ => typeof(Entity),
                    _ => typeof(EntityStream),
                    _ => typeof(DataStream)
                );
            }
        }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {
            Type stepType = typeof(Constant<>).MakeGenericType(ElementType);
            var stepAsObject = Activator.CreateInstance(stepType, Value.Value);

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
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError>
            TryGetVariablesSet(TypeResolver typeResolver) =>
            ImmutableList<(VariableName VariableName, ITypeReference type)>.Empty;

        /// <inheritdoc />
        public string StepName
        {
            get
            {
                var r = WriteValue(Value);
                return r;
            }
        }

        /// <summary>
        /// Serialize a value.
        /// </summary>
        public static string WriteValue(Option value)
        {
            return

                value.Match(
                    s => $"'{s}'",
                    i => i.ToString(),
                    d => d.ToString("G"),
                    b => b.ToString(),
                    e => e.ToString(),
                    dt => dt.ToString("O"),
                    entity =>
                    {
                        var r = entity.Serialize();
                        return r;
                    },
                    _ => "EntityStream", //TODO fix
                    ds => "DataStream" //TODO fix
                );
        }


        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver)
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

            var r= type.Map(x => new ActualTypeReference(x) as ITypeReference);


            return r;

            Result<Type,IError> GetEnumerationType(Enumeration enumeration)
            {
                if (typeResolver.StepFactoryStore.EnumTypesDictionary.TryGetValue(enumeration.Type, out var t))
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