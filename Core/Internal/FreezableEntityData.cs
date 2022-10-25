namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// The data used by a Freezable Step.
/// </summary>
public record FreezableEntityData(
    IReadOnlyDictionary<EntityNestedKey, FreezableStepProperty> EntityProperties,
    TextLocation Location)
{
    /// <inheritdoc />
    public override string ToString()
    {
        var keyString = string.Join("; ", EntityProperties);

        if (string.IsNullOrWhiteSpace(keyString))
            return "Empty";

        return keyString;
    }

    /// <summary>
    /// Gets the variables set by steps in this FreezableStepData.
    /// </summary>
    public Result<IReadOnlyCollection<UsedVariable>, IError>
        GetVariablesUsed(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var variables = new List<UsedVariable>();
        var errors    = new List<IError>();

        foreach (var stepProperty in EntityProperties)
        {
            switch (stepProperty.Value)
            {
                case FreezableStepProperty.Step step:
                    LocalGetVariablesSet(step.FreezableStep);
                    break;
                case FreezableStepProperty.StepList stepList:
                {
                    foreach (var step in stepList.List)
                        LocalGetVariablesSet(step);

                    break;
                }
                case FreezableStepProperty.Variable _: break;
                default:                               throw new ArgumentOutOfRangeException();
            }
        }

        if (errors.Any())
            return Result
                .Failure<IReadOnlyCollection<UsedVariable>,
                    IError>(ErrorList.Combine(errors));

        return variables;

        void LocalGetVariablesSet(IFreezableStep freezableStep)
        {
            var variablesSet = freezableStep.GetVariablesUsed(callerMetadata, typeResolver);

            if (variablesSet.IsFailure)
                errors.Add(variablesSet.Error);

            else
                variables.AddRange(variablesSet.Value);
        }
    }
}
