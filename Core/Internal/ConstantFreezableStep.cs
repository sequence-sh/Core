namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A constant string
/// </summary>
public record StringConstantFreezable
    (StringStream Value, TextLocation TextLocation) : ConstantFreezableBase<StringStream>(
        Value,
        TextLocation
    )
{
    /// <inheritdoc />
    public override string StepName => Value.GetString();

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => new StringConstant(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.Serialize();
}

/// <summary>
/// A constant int
/// </summary>
public record IntConstantFreezable
    (int Value, TextLocation TextLocation) : ConstantFreezableBase<int>(Value, TextLocation)
{
    /// <inheritdoc />
    public override string StepName => Value.ToString();

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        var intCheckResult = callerMetadata.CheckAllows(
            TypeReference.Actual.Integer,
            null
        );

        if (intCheckResult.IsSuccess)
            return new IntConstant(Value);

        var doubleCheckResult = callerMetadata.CheckAllows(
            TypeReference.Actual.Double,
            null
        );

        if (doubleCheckResult.IsSuccess)
            return new DoubleConstant(Value);

        return intCheckResult.MapError(x => x.WithLocation(this)).ConvertFailure<IStep>();
    }

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant double
/// </summary>
public record DoubleConstantFreezable
    (double Value, TextLocation TextLocation) : ConstantFreezableBase<double>(Value, TextLocation)
{
    /// <inheritdoc />
    public override string StepName => Value.ToString(Constants.DoubleFormat);

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => new DoubleConstant(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DoubleFormat);
}

/// <summary>
/// A constant bool
/// </summary>
public record BoolConstantFreezable
    (bool Value, TextLocation TextLocation) : ConstantFreezableBase<bool>(Value, TextLocation)
{
    /// <inheritdoc />
    public override string StepName => Value.ToString();

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => new BoolConstant(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();
}

/// <summary>
/// A constant DateTime
/// </summary>
public record DateTimeConstantFreezable
    (DateTime Value, TextLocation TextLocation) : ConstantFreezableBase<DateTime>(
        Value,
        TextLocation
    )
{
    /// <inheritdoc />
    public override string StepName => Value.ToString(Constants.DateTimeFormat);

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => new DateTimeConstant(Value);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString(Constants.DateTimeFormat);
}

/// <summary>
/// An Enum Constant
/// </summary>
public record EnumConstantFreezable
    (Enumeration Value, TextLocation TextLocation) : ConstantFreezableBase<Enumeration>(
        Value,
        TextLocation
    )
{
    /// <inheritdoc />
    public override string StepName => Value.ToString();

    /// <inheritdoc />
    public override Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        var type = TryGetType(typeResolver);

        if (type.IsFailure)
            return type.ConvertFailure<IStep>();

        if (Enum.TryParse(type.Value, Value.Value, true, out var o))
            return TryCreateEnumConstant(o!).MapError(x => x.WithLocation(this));

        return (SingleError)ErrorCode.UnexpectedEnumValue.ToErrorBuilder(Value.Type, Value.Value)
            .WithLocation(this);
    }

    /// <inheritdoc />
    public override Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => TryGetType(typeResolver)
        .Map(TypeReference.Create);

    /// <inheritdoc />
    public override string Serialize() => Value.ToString();

    private Result<Type, IError> TryGetType(TypeResolver typeResolver)
    {
        if (typeResolver.StepFactoryStore.EnumTypesDictionary.TryGetValue(Value.Type, out var t))
            return t;

        return (SingleError)ErrorCode.UnexpectedEnumType.ToErrorBuilder(Value.Type)
            .WithLocation(this);
    }

    /// <summary>
    /// Tries to create an enum constant from a value.
    /// Will fail if the value is not an enum.
    /// </summary>
    public static Result<IStep, IErrorBuilder> TryCreateEnumConstant(object value)
    {
        var type = value.GetType();

        if (!type.IsEnum)
            return new ErrorBuilder(ErrorCode.UnexpectedEnumType, type.Name);

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
            return ErrorCode.InvalidCast.ToErrorBuilder(e);
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
    string Serialize();
}

/// <summary>
/// The base class for freezable constants
/// </summary>
public abstract record ConstantFreezableBase<T>
    (T Value, TextLocation TextLocation) : IConstantFreezableStep
{
    /// <inheritdoc />
    public abstract string StepName { get; }

    /// <inheritdoc />
    public abstract Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver);

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return Array.Empty<UsedVariable>();
    }

    /// <inheritdoc />
    public virtual Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return TypeReference.Create(typeof(T));
    }

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore)
    {
        return this;
    }

    /// <inheritdoc />
    public bool Equals(IFreezableStep? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        var r = other is ConstantFreezableBase<T> cfs && Value!.Equals(cfs.Value);

        return r;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Value!.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => StepName;

    /// <inheritdoc />
    public object ValueObject => Value!;

    /// <inheritdoc />
    public abstract string Serialize();
}
