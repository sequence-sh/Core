using System;
using System.Collections.Generic;
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
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public abstract record ConstantBase<T>(T Value) : IStep<T>, IConstantStep
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        return Value;
    }

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
    public Result<Unit, IError> Verify(SCLSettings settings) => Unit.Default;

    /// <inheritdoc />
    public Configuration? Configuration { get; set; }

    /// <inheritdoc />
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public Type OutputType => typeof(T);

    /// <inheritdoc />
    public abstract string Serialize();

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            yield break;
        }
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => false;
}

/// <summary>
/// A Constant String
/// </summary>
public record StringConstant
    (StringStream Value) : ConstantBase<StringStream>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new StringConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.Serialize();
}

/// <summary>
/// A Constant int
/// </summary>
public record IntConstant(int Value) : ConstantBase<int>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new IntConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant double
/// </summary>
public record DoubleConstant(double Value) : ConstantBase<double>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new DoubleConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DoubleFormat);
}

/// <summary>
/// A constant bool
/// </summary>
public record BoolConstant(bool Value) : ConstantBase<bool>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new BoolConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant enum value
/// </summary>
public record EnumConstant<T>(T Value) : ConstantBase<T>(Value) where T : Enum
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze()
    {
        return new EnumConstantFreezable(
            new Enumeration(typeof(T).Name, Value.ToString()),
            TextLocation
        );
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
public record DateTimeConstant(DateTime Value) : ConstantBase<DateTime>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new DateTimeConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DateTimeFormat);
}

/// <summary>
/// A constant entity value
/// </summary>
public record EntityConstant(Entity Value) : ConstantBase<Entity>(Value)
{
    /// <inheritdoc />
    public override IFreezableStep Unfreeze() => new EntityConstantFreezable(Value, TextLocation);

    /// <inheritdoc />
    public override string Serialize() => Value.Serialize();
}

}
