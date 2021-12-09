namespace Reductech.EDR.Core.Internal.Serialization;

/// <summary>
/// Serializer for ArrayElementAtIndex
/// </summary>
public class ArrayElementAtIndexSerializer : IStepSerializer
{
    private ArrayElementAtIndexSerializer() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static IStepSerializer Instance { get; } = new ArrayElementAtIndexSerializer();

    /// <inheritdoc />
    public string Serialize(IEnumerable<StepProperty> stepProperties)
    {
        var (first, second) = stepProperties.GetFirstTwo().GetValueOrThrow();

        var entity = first.Serialize();

        var index = second.Serialize();

        return $"{entity}[{index}]";
    }
}
