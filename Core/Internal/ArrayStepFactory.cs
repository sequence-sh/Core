using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Step factory for types which manipulate arrays
/// </summary>
public abstract class ArrayStepFactory : GenericStepFactory
{
    /// <summary>
    /// Possible array output types
    /// </summary>
    protected abstract Result<TypeReference, IErrorBuilder> GetExpectedArrayTypeReference(
        TypeReference expectedTypeReference);

    /// <summary>
    /// The name of the array property
    /// </summary>
    protected abstract string ArrayPropertyName { get; }

    /// <inheritdoc />
    protected override Result<TypeReference, IError> GetMemberType(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        var expectedMemberTypeReference =
            GetExpectedArrayTypeReference(expectedTypeReference)
                .MapError(x => x.WithLocation(freezableStepData));

        if (expectedMemberTypeReference.IsFailure)
            return expectedMemberTypeReference.ConvertFailure<TypeReference>();

        var step = freezableStepData.TryGetStep(ArrayPropertyName, StepType);

        if (step.IsFailure)
            return step.ConvertFailure<TypeReference>();

        var outputTypeReference = step.Value.TryGetOutputTypeReference(
            expectedMemberTypeReference.Value,
            typeResolver
        );

        if (outputTypeReference.IsFailure)
            return outputTypeReference.ConvertFailure<TypeReference>();

        var arrayMemberType = outputTypeReference.Value
            .TryGetArrayMemberTypeReference(typeResolver)
            .MapError(e => e.WithLocation(freezableStepData));

        return arrayMemberType;
    }
}

}
