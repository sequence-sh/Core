using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal.Serialization;

/// <summary>
/// Serializer for steps with chain infix operators
/// </summary>
public record ChainInfixSerializer(string StepName, string Operator) : IStepSerializer
{
    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var properties = stepProperties as StepProperty[] ?? stepProperties.ToArray();

        if (properties.Length == 1 && properties.Single() is StepProperty.SingleStepProperty
            {
                Step: IArrayNewStep arrayNewStep
            } && arrayNewStep.ElementSteps.Count() > 1)
        {
            var text = string.Join(
                $" {Operator} ",
                arrayNewStep.ElementSteps.Select(
                    x =>
                    {
                        if (x.ShouldBracketWhenSerialized)
                        {
                            return $"({x.Serialize(options)})";
                        }

                        return x.Serialize(options);
                    }
                )
            );

            return text;
        }

        //This will happen if the source of elements is not an array directly
        return new FunctionSerializer(StepName).Serialize(options, properties);
    }
}
