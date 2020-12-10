using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that returns a constant value.
    /// </summary>
    public abstract class ConstantBase<T> : IStep<T>, IConstantStep
    {
        /// <summary>
        /// Create a new Constant
        /// </summary>
        protected ConstantBase(T value) => Value = value;

        /// <inheritdoc />
        public async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Value;
        }

        /// <summary>
        /// The value of this constant.
        /// </summary>
        public T Value { get; }

        /// <inheritdoc />
        public string Name => Value!.ToString()!;

        /// <inheritdoc />
        public object ValueObject => Value!;

        /// <inheritdoc />
        public abstract IFreezableStep Unfreeze();

        /// <inheritdoc />
        public async Task<Result<T1, IError>> Run<T1>(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var r = Value!.TryConvert<T1>()
                .MapError(x => new SingleError(x, ErrorCode.InvalidCast, new StepErrorLocation(this)) as IError);

            return r;
        }

        /// <inheritdoc />
        public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

        /// <inheritdoc />
        public Configuration? Configuration { get; set; } = null;

        /// <inheritdoc />
        public Type OutputType => typeof(T);

        /// <inheritdoc />
        public abstract Task<string> SerializeAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public bool ShouldBracketWhenSerialized => false;
    }

    /// <summary>
    /// A Constant String
    /// </summary>
    public class StringConstant : ConstantBase<StringStream>
    {
        /// <inheritdoc />
        public StringConstant(StringStream value) : base(value) { }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new StringConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await Value.SerializeAsync(cancellationToken);
    }


    /// <summary>
    /// A Constant int
    /// </summary>
    public class IntConstant : ConstantBase<int>
    {
        /// <inheritdoc />
        public IntConstant(int value) : base(value) { }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new IntConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString());
    }

    /// <summary>
    /// A constant double
    /// </summary>
    public class DoubleConstant : ConstantBase<double>
    {
        /// <inheritdoc />
        public DoubleConstant(double value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new DoubleConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString("G17"));
    }

    /// <summary>
    /// A constant bool
    /// </summary>
    public class BoolConstant : ConstantBase<bool>
    {
        /// <inheritdoc />
        public BoolConstant(bool value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new BoolConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString());
    }

    /// <summary>
    /// A constant enum value
    /// </summary>
    public class EnumConstant<T> : ConstantBase<T> where T : Enum
    {
        /// <inheritdoc />
        public EnumConstant(T value) : base(value){}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze()
        {
            return new EnumConstantFreezable(new Enumeration(typeof(T).Name, Value.ToString()));
        }

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken)
        {
            var enumeration = new Enumeration(typeof(T).Name, Value.ToString());
            return await ValueTask.FromResult(enumeration.ToString());
        }
    }




    /// <summary>
    /// A constant date time value
    /// </summary>
    public class DateTimeConstant : ConstantBase<DateTime>
    {
        /// <inheritdoc />
        public DateTimeConstant(DateTime value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new DateTimeConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.ToString("O"));
    }

    /// <summary>
    /// A constant entity value
    /// </summary>
    public class EntityConstant : ConstantBase<Entity>
    {
        /// <inheritdoc />
        public EntityConstant(Entity value) : base(value) {}

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new EntityConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await ValueTask.FromResult(Value.Serialize());
    }

    /// <summary>
    /// A constant entityStream value
    /// </summary>
    public class EntityStreamConstant : ConstantBase<EntityStream>
    {
        /// <inheritdoc />
        public EntityStreamConstant(EntityStream value) : base(value)
        {
        }

        /// <inheritdoc />
        public override IFreezableStep Unfreeze() => new EntityStreamConstantFreezable(Value);

        /// <inheritdoc />
        public override async Task<string> SerializeAsync(CancellationToken cancellationToken) => await Value.SerializeEntityStreamAsync(cancellationToken);
    }
}