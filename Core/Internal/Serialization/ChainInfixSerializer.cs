using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Core.Internal.Serialization;

/// <summary>
/// Serializer for steps with chain infix operators
/// </summary>
public record ChainInfixSerializer(string StepName, string Operator) : IStepSerializer
{
    /// <inheritdoc />
    public string Serialize(SerializeOptions options, IEnumerable<StepProperty> stepProperties)
    {
        var properties = stepProperties as StepProperty[] ?? stepProperties.ToArray();

        if (properties.Length != 1 || properties.Single() is not StepProperty.SingleStepProperty
            {
                Step: IArrayNewStep arrayNewStep
            } || arrayNewStep.ElementSteps.Count() <= 1)
            return new FunctionSerializer(StepName).Serialize(
                options,
                properties
            ); //Default to function serializer for this

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

    /// <inheritdoc />
    public void Format(
        IEnumerable<StepProperty> stepProperties,
        TextLocation? textLocation,
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        var properties = stepProperties as StepProperty[] ?? stepProperties.ToArray();

        if (properties.Length != 1 || properties.Single() is not StepProperty.SingleStepProperty
            {
                Step: IArrayNewStep arrayNewStep
            } || arrayNewStep.ElementSteps.Count() <= 1)
        {
            new FunctionSerializer(StepName).Format(
                properties,
                textLocation,
                indentationStringBuilder,
                options,
                remainingComments
            ); //Default to function serializer for this

            return;
        }

        indentationStringBuilder.AppendJoin(
            $" {Operator} ",
            true,
            arrayNewStep.ElementSteps,
            step =>
            {
                if (step.ShouldBracketWhenSerialized)
                    indentationStringBuilder.Append("(");

                step.Format(indentationStringBuilder, options, remainingComments);

                if (step.ShouldBracketWhenSerialized)
                    indentationStringBuilder.Append(")");
            }
        );
    }
}
