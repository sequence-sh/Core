using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Deserializes a regex group into a constant of any type.
/// </summary>
public class StepComponent : ISerializerBlock
{
    /// <summary>
    /// Creates a new StepComponent
    /// </summary>
    /// <param name="propertyName"></param>
    public StepComponent(string propertyName) => PropertyName = propertyName;

    /// <summary>
    /// The property name
    /// </summary>
    public string PropertyName { get; }

    /// <inheritdoc />
    public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary)
    {
        var property = dictionary[PropertyName];

        var r = property.Serialize();
        return r;
    }
}

}
