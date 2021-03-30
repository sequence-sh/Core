using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal.Serialization
{

/// <summary>
/// Serializer for steps with chain infix operators
/// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public record ChainInfixSerializer(string StepName, string Operator) : IStepSerializer
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
{
    /// <inheritdoc />
    public string Serialize(IEnumerable<StepProperty> stepProperties)
    {
        var properties = stepProperties as StepProperty[] ?? Enumerable.ToArray(stepProperties);

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
                            return $"({x.Serialize()})";
                        }

                        return x.Serialize();
                    }
                )
            );

            return text;
        }

        //This will happen if the source of elements is not an array directly
        return new FunctionSerializer(StepName).Serialize(properties);
    }
}

}
