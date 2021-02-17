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
    public override Result<ITypeReference, IError> TryGetOutputTypeReference(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver) => GetMemberType(freezableStepData, typeResolver)
        .Map(GetOutputTypeReference);

    /// <summary>
    /// Gets the output type from the member type.
    /// </summary>
    protected abstract ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference);

    /// <inheritdoc />
    protected override Result<ICompoundStep, IError> TryCreateInstance(
        TypeResolver typeResolver,
        FreezableStepData freezeData) => GetMemberType(freezeData, typeResolver)
        .Bind(
            x => typeResolver.TryGetTypeFromReference(x).MapError(e => e.WithLocation(freezeData))
        )
        .Bind(x => TryCreateGeneric(StepType, x).MapError(e => e.WithLocation(freezeData)));

    /// <summary>
    /// Gets the type
    /// </summary>
    protected abstract Result<ITypeReference, IError> GetMemberType(
        FreezableStepData freezableStepData,
        TypeResolver typeResolver);
}

}
