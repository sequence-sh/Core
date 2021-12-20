namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// A custom step serializer.
/// </summary>
public interface IStepSerializer
{
    /// <summary>
    /// SerializeAsync a step according to it's properties.
    /// </summary>
    string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties);
}
