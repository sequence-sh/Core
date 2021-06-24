using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Reductech.EDR.Core
{

/// <summary>
/// Additional configuration that may be needed in some use cases.
/// </summary>
[DataContract]
public sealed class Configuration : IEntityConvertible
{
    /// <summary>
    /// Additional requirements, beyond the default for this step.
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<Requirement>? AdditionalRequirements { get; set; }

    /// <summary>
    /// Tags that the target machine must have (defined in a the config file) for this to be run on that machine.
    /// </summary>
    [property: DataMember]
    public IReadOnlyList<string>? TargetMachineTags { get; set; }

    /// <summary>
    /// Conditional true, this step will not be split into multiple steps.
    /// </summary>
    [property: DataMember]
    public bool DoNotSplit { get; set; }

    /// <summary>
    /// The priority of this step. InitialSteps with higher priorities will be run first.
    /// </summary>
    [property: DataMember]
    public byte? Priority { get; set; }

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Configuration configuration && ToTuple.Equals(configuration.ToTuple);

    /// <inheritdoc />
    public override int GetHashCode() => ToTuple.GetHashCode();

    private object ToTuple => (
        string.Join(",", AdditionalRequirements ?? new List<Requirement>()),
        string.Join(",", TargetMachineTags ?? new List<string>()),
        DoNotSplit, Priority);
}

}
