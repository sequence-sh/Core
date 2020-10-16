using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Checks if an array is empty.
    /// </summary>
    public sealed class ArrayIsEmpty<T> : CompoundStep<bool>
    {
        /// <summary>
        /// The array to check for emptiness.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<bool, IRunErrors>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken).Map(x => !x.Any());
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayIsEmptyStepFactory.Instance;
    }

    /// <summary>
    /// Checks if an array is empty.
    /// </summary>
    public sealed class ArrayIsEmptyStepFactory : GenericStepFactory
    {
        private ArrayIsEmptyStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayIsEmptyStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayIsEmpty<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(ArrayIsEmpty<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0))
                .Map(x=> x as ITypeReference);
    }
}