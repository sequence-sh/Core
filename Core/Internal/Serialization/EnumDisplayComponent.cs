namespace Reductech.EDR.Core.Internal.Serialization;

/// <summary>
/// Deserializes a regex group into an enum using the display value of the enum.
/// </summary>
public class EnumDisplayComponent<T> : ISerializerBlock where T : Enum
{
    /// <summary>
    /// Creates a new EnumDisplayComponent
    /// </summary>
    /// <param name="propertyName"></param>
    public EnumDisplayComponent(string propertyName) => PropertyName = propertyName;

    /// <summary>
    /// The name of the property
    /// </summary>
    public string PropertyName { get; }

    /// <inheritdoc />
    public Result<string> TryGetSegmentText(IReadOnlyDictionary<string, StepProperty> dictionary)
    {
        return dictionary.TryFindOrFail(PropertyName, $"Missing Property {PropertyName}")
            .Bind(
                x =>
                    x switch
                    {
                        StepProperty.SingleStepProperty singleStepProperty => singleStepProperty
                            .Step is EnumConstant<T> cs
                            ? cs.Value.GetDisplayName()
                            : Result.Failure<string>("Operator is non constant step"),
                        StepProperty.StepListProperty => Result.Failure<string>(
                            "Operator is Step List"
                        ),
                        StepProperty.VariableNameProperty => Result.Failure<string>(
                            "Operator is VariableName"
                        ),
                        _ => throw new ArgumentOutOfRangeException(nameof(x))
                    }
            );
    }
}
