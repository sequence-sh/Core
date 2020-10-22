using System;
using System.Collections.Generic;
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
    /// Gets the first index of an element in an array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FirstIndexOfElement<T> : CompoundStep<int>
    {
        /// <summary>
        /// The array to check.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The element to look for.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Element { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken).Compose(() => Element.Run(stateMonad, cancellationToken))
                .Map(x => x.Item1.IndexOf(x.Item2));
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => FirstIndexOfElementStepFactory.Instance;
    }

    /// <summary>
    /// Gets the first index of an element in an array.
    /// </summary>
    public sealed class FirstIndexOfElementStepFactory : GenericStepFactory
    {
        private FirstIndexOfElementStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new FirstIndexOfElementStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(FirstIndexOfElement<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(FirstIndexOfElement<object>.Array), TypeName)
                .MapError(e=>e.WithLocation(this, freezableStepData))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x=>x.TryGetGenericTypeReference(typeResolver, 0)
                .MapError(e=>e.WithLocation(this, freezableStepData))
                )
                .Map(x=> x as ITypeReference);
    }
}