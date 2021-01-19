using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
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
    public async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
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
    public async Task<Result<T1, IError>> Run<T1>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var r = Value!.TryConvert<T1>()
            .MapError(x => x.WithLocation(this));

        return r;
    }

    /// <inheritdoc />
    public Result<Unit, IError> Verify(ISettings settings) => Unit.Default;

    /// <inheritdoc />
    public Configuration? Configuration { get; set; } = null;

    /// <inheritdoc />
    public Type OutputType => typeof(T);

    /// <inheritdoc />
    public abstract string Serialize();

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
    public override string Serialize() => Value.Serialize();
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
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant double
/// </summary>
public class DoubleConstant : ConstantBase<double>
{
    /// <inheritdoc />
    public DoubleConstant(double value) : base(value) { }

    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new DoubleConstantFreezable(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DoubleFormat);
}

/// <summary>
/// A constant bool
/// </summary>
public class BoolConstant : ConstantBase<bool>
{
    /// <inheritdoc />
    public BoolConstant(bool value) : base(value) { }

    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new BoolConstantFreezable(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant enum value
/// </summary>
public class EnumConstant<T> : ConstantBase<T> where T : Enum
{
    /// <inheritdoc />
    public EnumConstant(T value) : base(value) { }

    /// <inheritdoc />
    public override IFreezableStep Unfreeze()
    {
        return new EnumConstantFreezable(new Enumeration(typeof(T).Name, Value.ToString()));
    }

    /// <inheritdoc />
    public override string Serialize()
    {
        var enumeration = new Enumeration(typeof(T).Name, Value.ToString());
        return enumeration.ToString();
    }
}

/// <summary>
/// A constant date time value
/// </summary>
public class DateTimeConstant : ConstantBase<DateTime>
{
    /// <inheritdoc />
    public DateTimeConstant(DateTime value) : base(value) { }

    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new DateTimeConstantFreezable(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DateTimeFormat);
}

/// <summary>
/// A constant entity value
/// </summary>
public class EntityConstant : ConstantBase<Entity>
{
    /// <inheritdoc />
    public EntityConstant(Entity value) : base(value) { }

    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new EntityConstantFreezable(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.Serialize();
}

}
