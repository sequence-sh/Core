//namespace Reductech.EDR.Core.Internal.Errors
//{

///// <summary>
///// The step where the error originated
///// </summary>
//public class ErrorLocation : IErrorLocation
//{
//    /// <summary>
//    /// Creates a new ErrorLocation
//    /// </summary>
//    /// <param name="step"></param>
//    public ErrorLocation(IStep step) => Step = step;

//    /// <summary>
//    /// The step
//    /// </summary>
//    public IStep Step { get; }

//    /// <inheritdoc />
//    public string AsString => Step.Name;

//    /// <inheritdoc />
//    public bool Equals(IErrorLocation? other) => other is ErrorLocation sel
//                                              && Step.Unfreeze().Equals(sel.Step.Unfreeze());

//    /// <inheritdoc />
//    public override bool Equals(object? obj)
//    {
//        if (obj is null)
//            return false;

//        if (ReferenceEquals(this, obj))
//            return true;

//        if (obj.GetType() != GetType())
//            return false;

//        return Equals((IErrorLocation)obj);
//    }

//    /// <inheritdoc />
//    public override int GetHashCode() => Step.Unfreeze().GetHashCode();
//}

//}


