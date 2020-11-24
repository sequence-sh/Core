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
    /// Counts the elements in an array.
    /// </summary>
    public sealed class ArrayCount<T> : CompoundStep<int>
    {
        /// <summary>
        /// The array to count.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken).Map(x=>x.Count);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayCountStepFactory.Instance;
    }

    /// <summary>
    /// Counts the elements in an array.
    /// </summary>
    public sealed class ArrayCountStepFactory : GenericStepFactory
    {
        private ArrayCountStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayCountStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayCount<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Int32);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(int));

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {

            var r1 = freezableStepData.GetArgument(nameof(ArrayCount<object>.Array), TypeName)
                .MapError(x=>x.WithLocation(this, freezableStepData))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e=>e.WithLocation(this, freezableStepData)))
                .Map(x => x as ITypeReference);

            return r1;
        }
    }
}