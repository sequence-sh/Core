using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// A chain of steps to be run one after the other.
/// </summary>
public sealed class Sequence<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        foreach (var step in InitialSteps)
        {
            var r = await step.Run(stateMonad, cancellationToken);

            if (r.IsFailure)
                return r.ConvertFailure<T>();
        }

        var finalResult = await FinalStep.Run(stateMonad, cancellationToken);

        return finalResult;
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => SequenceStepFactory.Instance;

    /// <summary>
    /// The steps of this sequence apart from the final step.
    /// </summary>
    [StepListProperty(1)]
    [Required]
    public IReadOnlyList<IStep<Unit>> InitialSteps { get; set; } = null!;

    /// <summary>
    /// The final step of the sequence.
    /// Will be the return value.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> FinalStep { get; set; } = null!;
}

/// <summary>
/// A sequence of steps to be run one after the other.
/// </summary>
public sealed class SequenceStepFactory : GenericStepFactory
{
    private SequenceStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static StepFactory Instance { get; } = new SequenceStepFactory();

    /// <inheritdoc />
    public override Type StepType => typeof(Sequence<>);

    /// <inheritdoc />
    protected override TypeReference GetOutputTypeReference(TypeReference memberTypeReference) =>
        memberTypeReference;

    /// <inheritdoc />
    protected override Result<TypeReference, IError> GetMemberType(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver) => freezableStepData
        .TryGetStep(nameof(Sequence<object>.FinalStep), StepType)
        .Bind(x => x.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver));

    /// <inheritdoc />
    public override string OutputTypeExplanation => "The same type as the final step";

    /// <inheritdoc />
    public override IStepSerializer Serializer { get; } = SequenceSerializer.Instance;
}

}
