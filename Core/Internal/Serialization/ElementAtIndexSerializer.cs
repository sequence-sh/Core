using System.Collections.Generic;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Serializer for ElementAtIndex
/// </summary>
public class ElementAtIndexSerializer : IStepSerializer
{
    private ElementAtIndexSerializer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IStepSerializer Instance { get; } = new ElementAtIndexSerializer();

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
