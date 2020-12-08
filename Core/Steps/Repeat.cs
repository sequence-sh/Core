using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class Repeat<T> : CompoundStep<List<T>>
    {
        /// <summary>
        /// The element to repeat.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<T> Element { get; set; } = null!;

        /// <summary>
        /// The number of times to repeat the element
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<int> Number { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<List<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await Element.Run(stateMonad, cancellationToken).Compose(() => Number.Run(stateMonad, cancellationToken))
                .Map(x => Enumerable.Repeat(x.Item1, x.Item2).ToList());
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => RepeatStepFactory.Instance;
    }

    /// <summary>
    /// Creates an array by repeating an element.
    /// </summary>
    public sealed class RepeatStepFactory : GenericStepFactory
    {
        private RepeatStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new RepeatStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Repeat<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new[] { memberTypeReference });

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(Repeat<object>.Element), TypeName)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver));
    }
}