using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using DateTime = System.DateTime;
using Option = OneOf.OneOf<Reductech.EDR.Core.Parser.StringStream, int, double, bool,
Reductech.EDR.Core.Internal.Enumeration, System.DateTime,
Reductech.EDR.Core.Entity,
Reductech.EDR.Core.Entities.EntityStream>;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A constant string
    /// </summary>
    public class StringConstantFreezable : ConstantFreezableBase<StringStream>
    {
        /// <inheritdoc />
        public StringConstantFreezable(StringStream value) : base(value) { }

        /// <inheritdoc />
        public override string StepName => Value.Name;

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new StringConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Value.SerializeAsync(cancellation);
    }

    /// <summary>
    /// A constant int
    /// </summary>
    public class IntConstantFreezable : ConstantFreezableBase<int>
    {
        /// <inheritdoc />
        public IntConstantFreezable(int value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => Value.ToString();

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new IntConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString());
    }

    /// <summary>
    /// A constant double
    /// </summary>
    public class DoubleConstantFreezable : ConstantFreezableBase<double>
    {
        /// <inheritdoc />
        public DoubleConstantFreezable(double value) : base(value) { }

        /// <inheritdoc />
        public override string StepName => Value.ToString("G17");

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new DoubleConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString("G17"));
    }

    /// <summary>
    /// A constant bool
    /// </summary>
    public class BoolConstantFreezable : ConstantFreezableBase<bool>
    {
        /// <inheritdoc />
        public BoolConstantFreezable(bool value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => Value.ToString();

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new BoolConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString());
    }


    /// <summary>
    /// A constant DateTime
    /// </summary>
    public class DateTimeConstantFreezable : ConstantFreezableBase<DateTime>
    {
        /// <inheritdoc />
        public DateTimeConstantFreezable(DateTime value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => Value.ToString("O");

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new DateTimeConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString("O"));
    }

    /// <summary>
    /// An entity Constant
    /// </summary>
    public class EntityConstantFreezable : ConstantFreezableBase<Entity>
    {
        /// <inheritdoc />
        public EntityConstantFreezable(Entity value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => Value.Serialize();

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new EntityConstant(Value);

        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString());
    }

    /// <summary>
    /// An entityStream Constant
    /// </summary>
    public class EntityStreamConstantFreezable : ConstantFreezableBase<EntityStream>
    {
        /// <inheritdoc />
        public EntityStreamConstantFreezable(EntityStream value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => "EntityStream";

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext) => new EntityStreamConstant(Value);

        /// <param name="cancellation"></param>
        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Value.SerializeEntityStreamAsync(cancellation);
    }

    /// <summary>
    /// An Enum Constant
    /// </summary>
    public class EnumConstantFreezable : ConstantFreezableBase<Enumeration>
    {
        /// <inheritdoc />
        public EnumConstantFreezable(Enumeration value) : base(value) {}

        /// <inheritdoc />
        public override string StepName => Value.ToString();

        /// <inheritdoc />
        public override Result<IStep, IError> TryFreeze(StepContext stepContext)
        {
            var type = TryGetType(stepContext.TypeResolver);
            if (type.IsFailure) return type.ConvertFailure<IStep>();

            if (Enum.TryParse(type.Value, Value.Value, false, out var o))
                return TryCreateEnumConstant(o!).MapError(x => x.WithLocation(this));

            return new SingleError($"Enum '{Value.Type}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
        }


        /// <inheritdoc />
        public override Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) =>
            TryGetType(typeResolver).Map(x => new ActualTypeReference(x) as ITypeReference);

        /// <param name="cancellation"></param>
        /// <inheritdoc />
        public override Task<string> SerializeAsync(CancellationToken cancellation) => Task.FromResult(Value.ToString());

        private Result<Type, IError> TryGetType(TypeResolver typeResolver)
        {
            if (typeResolver.StepFactoryStore.EnumTypesDictionary.TryGetValue(Value.Type, out var t))
                return t;
            return new SingleError($"Enum '{Value.Type}' does not exist", ErrorCode.UnexpectedEnumValue, new FreezableStepErrorLocation(this));
        }


        /// <summary>
        /// Tries to create an enum constant from a value.
        /// Will fail if the value is not an enum.
        /// </summary>
        public static Result<IStep, IErrorBuilder> TryCreateEnumConstant(object value)
        {
            var type = value.GetType();

            if (!type.IsEnum)
                return new ErrorBuilder($"{type.Name} is not an enum type", ErrorCode.InvalidCast);

            Type stepType = typeof(EnumConstant<>).MakeGenericType(type);

            var stepAsObject = Activator.CreateInstance(stepType, value);

            try
            {
                var step = (IStep)stepAsObject!;
                return Result.Success<IStep, IErrorBuilder>(step);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                return new ErrorBuilder(e, ErrorCode.InvalidCast);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }

    /// <summary>
    /// A freezable step which represents a constant value.
    /// </summary>
    public interface IConstantFreezableStep : IFreezableStep
    {
        /// <summary>
        /// The Constant Value
        /// </summary>
        object ValueObject { get; }

        /// <summary>
        /// Serialize this constant
        /// </summary>
        /// <param name="cancellation"></param>
        Task<string> SerializeAsync(CancellationToken cancellation);

    }

    /// <summary>
    /// The base class for freezable constants
    /// </summary>
    public abstract class ConstantFreezableBase<T> : IConstantFreezableStep
    {
        /// <summary>
        /// Create a new ConstantFreezable
        /// </summary>
        protected ConstantFreezableBase(T value) => Value = value;

        /// <summary>
        /// The constant value
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
        public abstract string StepName { get; }

        /// <inheritdoc />
        public abstract Result<IStep, IError> TryFreeze(StepContext stepContext);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            return new List<(VariableName variableName, Maybe<ITypeReference>)>();
        }

        /// <inheritdoc />
        public virtual Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver)
        {
            return new ActualTypeReference(typeof(T));
        }

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            var r = other is ConstantFreezableBase<T> cfs && Value!.Equals(cfs.Value);

            return r;
        }


        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Value!.GetHashCode();

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public object ValueObject => Value!;

        /// <param name="cancellation"></param>
        /// <inheritdoc />
        public abstract Task<string> SerializeAsync(CancellationToken cancellation);
    }
}