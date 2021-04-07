using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{

/// <summary>
/// Returns the consequent if a condition is true and the alternative if the condition is false.
/// </summary>
public sealed class ValueIf<T> : CompoundStep<T>
{
    /// <inheritdoc />
    protected override async Task<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var result = await Condition.Run(stateMonad, cancellationToken)
            .Bind(
                r => r
                    ? Then.Run(stateMonad, cancellationToken)
                    : Else.Run(stateMonad, cancellationToken)
            );

        return result;
    }

    /// <summary>
    /// Whether to follow the Then Branch
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<bool> Condition { get; set; } = null!;

    /// <summary>
    /// The Consequent. Returned if the condition is true.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Then { get; set; } = null!;

    /// <summary>
    /// The Alternative. Returned if the condition is false.
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<T> Else { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ValueIfStepFactory.Instance;

    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    private sealed class ValueIfStepFactory : GenericStepFactory
    {
        private ValueIfStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ValueIfStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ValueIf<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            TypeReference expectedTypeReference,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver) => freezableStepData
            .TryGetStep(nameof(Then), StepType)
            .Compose(() => freezableStepData.TryGetStep(nameof(Else), StepType))
            .Bind(
                x => x.Item1.TryGetOutputTypeReference(TypeReference.Any.Instance, typeResolver)
                    .Compose(
                        () => x.Item2.TryGetOutputTypeReference(
                            TypeReference.Any.Instance,
                            typeResolver
                        )
                    )
            )
            .Map(x => TypeReference.Create(new[] { x.Item1, x.Item2 }));
    }
}

}
