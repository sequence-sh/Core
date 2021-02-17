using System.Collections.Generic;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Serializer for EntityGetValue
/// </summary>
public class EntityGetValueSerializer : IStepSerializer
{
    private EntityGetValueSerializer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IStepSerializer Instance { get; } = new EntityGetValueSerializer();

    /// <inheritdoc />
    public string Serialize(IEnumerable<StepProperty> stepProperties)
    {
        var (first, second) = stepProperties.GetFirstTwo().Value;

        var entity = first.Serialize();

        var index = second.Serialize();

        return $"{entity}[{index}]";
    }
}

}
