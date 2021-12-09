using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A step that returns a constant value.
/// </summary>
public abstract record ConstantBase<T>(T Value) : IStep<T>, IConstantStep
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
    public abstract string Name { get; }

    /// <inheritdoc />
    public object ValueObject => Value!;

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
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore) => Unit.Default;

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
    public Maybe<EntityValue> TryConvertToEntityValue() => ToEntityValue();

    /// <summary>
    /// Converts this to an EntityValue
    /// </summary>
    /// <returns></returns>
    protected abstract EntityValue ToEntityValue();

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
    public override string Serialize() => Value.Serialize();

    /// <inheritdoc />
    public override string Name => Value.GetString();

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.String(Value.GetString());
}

/// <summary>
/// A Constant int
/// </summary>
public record IntConstant(int Value) : ConstantBase<int>(Value)
{
    /// <inheritdoc />
    public override string Serialize() => Value.ToString();

    /// <inheritdoc />
    public override string Name => Value.ToString();

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Integer(Value);
}

/// <summary>
/// A constant double
/// </summary>
public record DoubleConstant(double Value) : ConstantBase<double>(Value)
{
    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DoubleFormat);

    /// <inheritdoc />
    public override string Name => Value.ToString(Constants.DoubleFormat);

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Double(Value);
}

/// <summary>
/// A constant bool
/// </summary>
public record BoolConstant(bool Value) : ConstantBase<bool>(Value)
{
    /// <inheritdoc />
    public override string Serialize() => Value.ToString();

    /// <inheritdoc />
    public override string Name => Value.ToString();

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Boolean(Value);
}

/// <summary>
/// A constant enum value
/// </summary>
public record EnumConstant<T>(T Value) : ConstantBase<T>(Value) where T : Enum
{
    /// <inheritdoc />
    public override string Serialize()
    {
        return ToEnumeration().ToString();
    }

    /// <inheritdoc />
    public override string Name => ToEnumeration().Value;

    private Enumeration ToEnumeration() => new(typeof(T).Name, Value.ToString());

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() =>
        new EntityValue.EnumerationValue(ToEnumeration());
}

/// <summary>
/// A constant date time value
/// </summary>
public record DateTimeConstant(DateTime Value) : ConstantBase<DateTime>(Value)
{
    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    public override string Name => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.DateTime(Value);
}

/// <summary>
/// A constant entity value
/// </summary>
public record EntityConstant(Entity Value) : ConstantBase<Entity>(Value)
{
    /// <inheritdoc />
    public override string Serialize() => Value.Serialize();

    /// <inheritdoc />
    public override string Name => Value.Serialize();

    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.NestedEntity(Value);
}
