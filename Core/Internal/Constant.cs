namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A step that returns a constant value.
/// </summary>
public abstract record ConstantBase<T>(T Value)
    : IStep<T>, IConstantStep where T : ISCLObject
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
    public string Name => Value.Name;

    /// <inheritdoc />
    public async Task<Result<T1, IError>> Run<T1>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) where T1 : ISCLObject
    {
        await Task.CompletedTask;

        var r = Value.TryConvert<T1>()
            .MapError(x => x.WithLocation(this));

        return r;
    }

    ISCLObject IConstantStep.Value => Value;

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore) => Unit.Default;

    /// <inheritdoc />
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public Type OutputType => typeof(T);

    /// <inheritdoc />
    public string Serialize() => Value.Serialize();

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
    [Obsolete]
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
    protected override EntityValue ToEntityValue() => new EntityValue.String(Value.GetString());
}

/// <summary>
/// A Constant int
/// </summary>
public record IntConstant(SCLInt Value) : ConstantBase<SCLInt>(Value)
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Integer(Value.Value);
}

/// <summary>
/// A constant double
/// </summary>
public record DoubleConstant(SCLDouble Value) : ConstantBase<SCLDouble>(Value)
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Double(Value.Value);
}

/// <summary>
/// A constant bool
/// </summary>
public record BoolConstant(SCLBool Value) : ConstantBase<SCLBool>(Value)
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.Boolean(Value.Value);
}

/// <summary>
/// A constant enum value
/// </summary>
public record EnumConstant<TEnum>(SCLEnum<TEnum> Value) : ConstantBase<SCLEnum<TEnum>>(Value)
    where TEnum : Enum
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.EnumerationValue(Value);
}

/// <summary>
/// A constant date time value
/// </summary>
public record DateTimeConstant(SCLDateTime Value) : ConstantBase<SCLDateTime>(Value)
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.DateTime(Value.Value);
}

/// <summary>
/// A constant entity value
/// </summary>
public record EntityConstant(Entity Value) : ConstantBase<Entity>(Value)
{
    /// <inheritdoc />
    protected override EntityValue ToEntityValue() => new EntityValue.NestedEntity(Value);
}
