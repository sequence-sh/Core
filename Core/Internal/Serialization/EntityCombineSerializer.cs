using System.Collections.Generic;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Serializer for EntityCombine
/// </summary>
public class EntityCombineSerializer : IStepSerializer
{
    private EntityCombineSerializer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IStepSerializer Instance { get; } = new EntityCombineSerializer();

    /// <inheritdoc />
    public string Serialize(IEnumerable<StepProperty> stepProperties)
    {
        var (first, second) = stepProperties.GetFirstTwo().Value;

        var firstS = first.Serialize();

        var secondS = second.Serialize();

        return $"{firstS} with {secondS}";
    }
}

}
