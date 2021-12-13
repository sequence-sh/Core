namespace Reductech.EDR.Core.Internal;

///// <summary>
///// A constant string
///// </summary>
//public record StringConstantFreezable
//    (StringStream Value, TextLocation TextLocation) : ConstantFreezableBase<StringStream>(
//        Value,
//        TextLocation
//    )
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver) => new SCLConstant<>(Value);
//}

///// <summary>
///// A constant int
///// </summary>
//public record IntConstantFreezable
//    (SCLInt Value, TextLocation TextLocation) : ConstantFreezableBase<SCLInt>(Value, TextLocation)
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver)
//    {
//        var intCheckResult = callerMetadata.CheckAllows(
//            TypeReference.Actual.Integer,
//            null
//        );

//        if (intCheckResult.IsSuccess)
//            return new SCLConstant<SCLInt>(Value);

//        var doubleCheckResult = callerMetadata.CheckAllows(
//            TypeReference.Actual.Double,
//            null
//        );

//        if (doubleCheckResult.IsSuccess)
//            return new DoubleConstant(new SCLDouble(Value.Value));

//        return intCheckResult.MapError(x => x.WithLocation(this)).ConvertFailure<IStep>();
//    }
//}

///// <summary>
///// A constant double
///// </summary>
//public record DoubleConstantFreezable
//    (SCLDouble Value, TextLocation TextLocation) : ConstantFreezableBase<SCLDouble>(
//        Value,
//        TextLocation
//    )
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver) => new DoubleConstant(Value);
//}

///// <summary>
///// A constant bool
///// </summary>
//public record BoolConstantFreezable
//    (SCLBool Value, TextLocation TextLocation) : ConstantFreezableBase<SCLBool>(Value, TextLocation)
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver) => new SCLConstant<SCLBool>(Value);
//}

///// <summary>
///// A constant DateTime
///// </summary>
//public record DateTimeConstantFreezable
//    (SCLDateTime Value, TextLocation TextLocation) : ConstantFreezableBase<SCLDateTime>(
//        Value,
//        TextLocation
//    )
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver) => new SCLConstant<SCLDateTime>(Value);
//}

///// <summary>
///// An Enum Constant
///// </summary>
//public record EnumConstantFreezable //TODO add this back???
//    (ISCLEnum Value, TextLocation TextLocation) : ConstantFreezableBase<ISCLEnum>(
//        Value,
//        TextLocation
//    )
//{
//    /// <inheritdoc />
//    public override Result<IStep, IError> TryFreeze(
//        CallerMetadata callerMetadata,
//        TypeResolver typeResolver)
//    {
//        return new EnumConstant(Value);

//        //var type = TryGetType(typeResolver);

//        //if (type.IsFailure)
//        //    return type.ConvertFailure<IStep>();

//        //if (Enum.TryParse(type.Value, Value.Value, true, out var o))
//        //    return TryCreateEnumConstant(o!).MapError(x => x.WithLocation(this));

//        //return (SingleError)ErrorCode.UnexpectedEnumValue.ToErrorBuilder(Value.Type, Value.Value)
//        //    .WithLocation(this);
//    }

//    ///// <inheritdoc />
//    //public override Result<TypeReference, IError> TryGetOutputTypeReference(
//    //    CallerMetadata callerMetadata,
//    //    TypeResolver typeResolver) => TryGetType(typeResolver)
//    //    .Map(TypeReference.Create);

//    //private Result<Type, IError> TryGetType(TypeResolver typeResolver)
//    //{
//    //    if (typeResolver.StepFactoryStore.EnumTypesDictionary.TryGetValue(Value.Type, out var t))
//    //        return t;

//    //    return (SingleError)ErrorCode.UnexpectedEnumType.ToErrorBuilder(Value.Type)
//    //        .WithLocation(this);
//    //}
//}

/// <summary>
/// A freezable step which represents a constant value.
/// </summary>
public interface IConstantFreezableStep : IFreezableStep
{
    /// <summary>
    /// The Constant Value
    /// </summary>
    ISCLObject ValueObject { get; }

    /// <summary>
    /// Serialize this constant
    /// </summary>
    string Serialize();
}

/// <summary>
/// The base class for freezable constants
/// </summary>
public sealed record SCLConstantFreezable<T>
    (T Value, TextLocation TextLocation) : IConstantFreezableStep where T : ISCLObject
{
    /// <inheritdoc />
    public string StepName => Value.Name;

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) => new SCLConstant<T>(Value) { TextLocation = TextLocation };

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>,
            IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        return Array.Empty<UsedVariable>();
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return Value.TypeReference;
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

        var r = other is IConstantFreezableStep cfs && Value.Equals(cfs.ValueObject);

        return r;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => StepName;

    /// <inheritdoc />
    public ISCLObject ValueObject => Value;

    /// <inheritdoc />
    public string Serialize() => Value.Serialize();
}
