namespace Sequence.Core.Internal;

/// <summary>
/// A discriminated union of SCL objects
/// </summary>
public interface ISCLOneOf : ISCLObject
{
    /// <summary>
    /// The actual value of this OneOf
    /// </summary>
    ISCLObject Value { get; }
}
