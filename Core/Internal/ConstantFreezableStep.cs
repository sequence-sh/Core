using Generator.Equals;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A freezable step which represents a constant value.
/// </summary>
public interface IConstantFreezableStep : IFreezableStep
{
    /// <summary>
    /// The Constant Value of this Step
    /// </summary>
    ISCLObject Value { get; }
}

/// <summary>
/// The base class for freezable constants
/// </summary>
[Equatable]
public sealed partial record SCLConstantFreezable<T>
    (T Value, [property: IgnoreEquality] TextLocation TextLocation) : IConstantFreezableStep
    where T : ISCLObject
{
    /// <inheritdoc />
    public string StepName => Value.Serialize(SerializeOptions.Primitive);

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return CheckFreezePossible(callerMetadata, typeResolver)
            .Map(() => new SCLConstant<T>(Value) { TextLocation = TextLocation } as IStep);
    }

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
        return Value.GetTypeReference();
    }

    /// <inheritdoc />
    public UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        if (callerMetadata.ExpectedType.Allow(Value.GetTypeReference(), typeResolver))
            return UnitResult.Success<IError>();

        var r = callerMetadata.ExpectedType.TryGetType(typeResolver)
            .Bind(type => Value.TryConvert(type, callerMetadata.ParameterName))
            .Map(_ => Unit.Default)
            .MapError(x => x.WithLocation(this));

        return r;
    }

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore)
    {
        return this;
    }

    /// <inheritdoc />
    public bool Equals(IFreezableStep? other)
    {
        return other is SCLConstantFreezable<T> constant && Equals(constant);
    }

    /// <inheritdoc />
    public override string ToString() => StepName;

    ISCLObject IConstantFreezableStep.Value => Value;
}
