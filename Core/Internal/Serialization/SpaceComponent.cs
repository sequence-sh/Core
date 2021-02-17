using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Include a required space in serialization.
/// </summary>
public class SpaceComponent : ISerializerBlock
{
    private SpaceComponent() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static SpaceComponent Instance { get; } = new();

    /// <inheritdoc />
    public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary) =>
        " ";
}

}
