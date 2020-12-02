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
        public override async Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await Condition.Run(stateMonad, cancellationToken)
                .Bind(r => r ? Then.Run(stateMonad, cancellationToken) : Else.Run(stateMonad, cancellationToken));

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ValueIfStepFactory.Instance;


        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Consequent. Returned if the condition is true.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Then { get; set; } = null!;

        /// <summary>
        /// The Alternative. Returned if the condition is false.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Else { get; set; } = null!;
    }

    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class ValueIfStepFactory : GenericStepFactory
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
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(ValueIf<object>.Then), TypeName)
                .Compose(() => freezableStepData.GetStep(nameof(ValueIf<object>.Else), TypeName))
                .Bind(x => x.Item1.TryGetOutputTypeReference(typeResolver)
                    .Compose(() => x.Item2.TryGetOutputTypeReference(typeResolver)))
                .Bind(x => MultipleTypeReference.TryCreate(new[] { x.Item1, x.Item2 }, TypeName)
                    .MapError(e=>e.WithLocation(this, freezableStepData)))
        ;
    }
}