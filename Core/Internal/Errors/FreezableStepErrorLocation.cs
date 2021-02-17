//namespace Reductech.EDR.Core.Internal.Errors
//{

///// <summary>
///// The freezable step where the error originated
///// </summary>
//public class FreezableErrorLocation : ErrorLocation
//{
//    /// <summary>
//    /// Creates a new FreezableErrorLocation
//    /// </summary>
//    public FreezableErrorLocation(IFreezableStep freezableStep) =>
//        FreezableStep = freezableStep;

//    /// <summary>
//    /// Creates a new FreezableErrorLocation
//    /// </summary>
//    public FreezableErrorLocation(IStepFactory stepFactory, FreezableStepData data) =>
//        FreezableStep = new CompoundFreezableStep(stepFactory.TypeName, data, null);

//    /// <summary>
//    /// The freezable step
//    /// </summary>
//    public IFreezableStep FreezableStep { get; }

//    /// <inheritdoc />
//    public string AsString => FreezableStep.StepName;

//    /// <inheritdoc />
//    public bool Equals(ErrorLocation? other)
//    {
//        if (other is null)
//            return false;

//        if (ReferenceEquals(this, other))
//            return true;

//        return other is FreezableErrorLocation y && FreezableStep.Equals(y.FreezableStep);
//    }

//    /// <inheritdoc />
//    public override bool Equals(object? obj)
//    {
//        if (obj is null)
//            return false;

//        if (ReferenceEquals(this, obj))
//            return true;

//        return obj is IErrorLocation errorLocation && Equals(errorLocation);
//    }

//    /// <inheritdoc />
//    public override int GetHashCode() => FreezableStep.StepName.GetHashCode();
//}

//}


