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
}
