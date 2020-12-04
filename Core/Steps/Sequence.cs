using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class Sequence<T> : CompoundStep<T>
    {
        /// <inheritdoc />
        public override async Task<Result<T, IError>> Run(IStateMonad stateMonad,
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
        [StepListProperty]
        [Required]
        public IReadOnlyList<IStep<Unit>> InitialSteps { get; set; } = null!;

        /// <summary>
        /// The final step of the sequence.
        /// Will be the return value.
        /// </summary>
        [StepListProperty]
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
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(Sequence<object>.FinalStep), TypeName)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver));

        /// <inheritdoc />
        public override string OutputTypeExplanation => "The same type as the final step";

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = SequenceSerializer.Instance;

        /// <summary>
        /// Create a new Freezable Sequence
        /// </summary>
        public static IFreezableStep CreateFreezable(IEnumerable<IFreezableStep> steps, IFreezableStep finalStep, Configuration? configuration, IErrorLocation location)
        {
            var dict = new Dictionary<string, FreezableStepProperty>()
            {
                {nameof(Sequence<object>.InitialSteps), new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT2(steps.ToList()), location )},
                {nameof(Sequence<object>.FinalStep), new FreezableStepProperty(OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(finalStep), location )},
            };

            var fpd = new FreezableStepData( dict, location);


            return new CompoundFreezableStep(Instance.TypeName, fpd, configuration);
        }
    }
}