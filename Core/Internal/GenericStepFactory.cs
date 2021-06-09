using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Step factory for generic types.
/// </summary>
public abstract class GenericStepFactory : StepFactory
{
    /// <inheritdoc />
    public override Result<TypeReference, IError> TryGetOutputTypeReference(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver) => GetGenericTypeParameter(
            expectedTypeReference,
            freezableStepData,
            typeResolver
        )
        .Map(GetOutputTypeReference);

    /// <summary>
    /// Gets the output type from the member type.
    /// </summary>
    protected abstract TypeReference GetOutputTypeReference(TypeReference memberTypeReference);

    /// <inheritdoc />
    protected override Result<ICompoundStep, IError> TryCreateInstance(
        TypeReference expectedTypeReference,
        FreezableStepData freezeData,
        TypeResolver typeResolver)
    {
        var genericTypeParameter = GetGenericTypeParameter(
            expectedTypeReference,
            freezeData,
            typeResolver
        );

        if (genericTypeParameter.IsFailure)
        {
            var firstError = genericTypeParameter.Error.GetAllErrors().First();

            if (firstError.ErrorBuilder.ErrorCode
             == ErrorCode.CannotInferType) //Get a more specific error
                return ErrorCode.WrongOutputType.ToErrorBuilder(
                        TypeName,
                        OutputTypeExplanation,
                        expectedTypeReference.Name
                    )
                    .WithLocationSingle(firstError.Location);

            return genericTypeParameter.ConvertFailure<ICompoundStep>();
        }

        var result = genericTypeParameter.Value.TryGetType(typeResolver)
            .Bind(x => TryCreateGeneric(StepType, x))
            .MapError(e => e.WithLocation(freezeData));

        return result;
    }

    /// <summary>
    /// Gets the type
    /// </summary>
    protected abstract Result<TypeReference, IError> GetGenericTypeParameter(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver);
}

}
