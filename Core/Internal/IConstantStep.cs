namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A step that returns a fixed value when run.
/// </summary>
public interface IConstantStep : IStep
{
    /// <summary>
    /// The constant value
    /// </summary>
    ISCLObject Value { get; }

    /// <summary>
    /// Try to convert this constant to a constant of a different type.
    /// </summary>
    public Result<IStep, IErrorBuilder> TryConvert(Type memberType, string propertyName);
}
