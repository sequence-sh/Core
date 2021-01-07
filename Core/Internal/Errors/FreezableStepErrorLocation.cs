namespace Reductech.EDR.Core.Internal.Errors
{

/// <summary>
/// The freezable step where the error originated
/// </summary>
public class FreezableStepErrorLocation : IErrorLocation
{
    /// <summary>
    /// Creates a new FreezableStepErrorLocation
    /// </summary>
    public FreezableStepErrorLocation(IFreezableStep freezableStep) =>
        FreezableStep = freezableStep;

    /// <summary>
    /// Creates a new FreezableStepErrorLocation
    /// </summary>
    public FreezableStepErrorLocation(IStepFactory stepFactory, FreezableStepData data) =>
        FreezableStep = new CompoundFreezableStep(stepFactory.TypeName, data, null);

    /// <summary>
    /// The freezable step
    /// </summary>
    public IFreezableStep FreezableStep { get; }

    /// <inheritdoc />
    public string AsString => FreezableStep.StepName;

    /// <inheritdoc />
    public bool Equals(IErrorLocation? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other is FreezableStepErrorLocation y && FreezableStep.Equals(y.FreezableStep);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is IErrorLocation errorLocation && Equals(errorLocation);
    }

    /// <inheritdoc />
    public override int GetHashCode() => FreezableStep.StepName.GetHashCode();
}

}
