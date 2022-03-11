namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A parameter of a step
/// </summary>
public interface IStepParameter
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The step type of the parameter.
    /// </summary>
    Type StepType { get; }

    /// <summary>
    /// The output type of the parameter step
    /// </summary>
    Type ActualType { get; }

    /// <summary>
    /// Aliases for this parameter
    /// </summary>
    IReadOnlyCollection<string> Aliases { get; }

    /// <summary>
    /// Whether this parameter is required.
    /// </summary>
    bool Required { get; }

    /// <summary>
    /// /// A summary of what this parameter does.
    /// </summary>
    string Summary { get; }

    /// <summary>
    /// Additional Fields  Examples, Default Values, Requirements etc.
    /// </summary>
    IReadOnlyDictionary<string, string> ExtraFields { get; }

    /// <summary>
    /// The index of this parameter starting at one.
    /// </summary>
    int? Order { get; }

    /// <summary>
    /// The type of this parameter
    /// </summary>
    MemberType MemberType { get; }

    /// <summary>
    /// Metadata set by the MetadataAttribute.
    /// Case Insensitive
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
