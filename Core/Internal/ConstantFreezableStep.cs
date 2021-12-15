namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A freezable step which represents a constant value.
/// </summary>
public interface IConstantFreezableStep : IFreezableStep { }

/// <summary>
/// The base class for freezable constants
/// </summary>
public sealed record SCLConstantFreezable<T>
    (T Value, TextLocation TextLocation) : IConstantFreezableStep where T : ISCLObject
{
    /// <inheritdoc />
    public string StepName => Value.Serialize(SerializeOptions.Primitive);

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
        return Value.GetTypeReference();
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

        var r = other is SCLConstantFreezable<T> cfs && Value.Equals(cfs.ValueObject);

        return r;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => StepName;

    /// <inheritdoc />
    public ISCLObject ValueObject => Value;
}
