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
    /// Reorder an array.
    /// </summary>
    public sealed class ArraySort<T> : CompoundStep<List<T>>
    {
        /// <summary>
        /// The array to modify.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// Whether to sort in descending order.
        /// </summary>
        [StepProperty(2)]
        [DefaultValueExplanation("False")]
        public IStep<bool> Descending { get; set; } = new Constant<bool>(false);

        /// <inheritdoc />
        public override async Task<Result<List<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken)
                .Compose(() => Descending.Run(stateMonad, cancellationToken))
                .Map(x => Sort(x.Item1, x.Item2));
        }

        private static List<T> Sort(IEnumerable<T> list, bool descending) =>

            descending?list.OrderByDescending(x => x).ToList():
                list.OrderBy(x => x).ToList();

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArraySortStepFactory.Instance;
    }


    /// <summary>
    /// Reorder an array.
    /// </summary>
    public sealed class ArraySortStepFactory : GenericStepFactory
    {
        private ArraySortStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArraySortStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArraySort<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new[] { memberTypeReference });

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";


        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetStep(nameof(ArraySort<object>.Array), TypeName)
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e=>e.WithLocation(this, freezableStepData)))
                .Map(x => x as ITypeReference)
        ;
    }
}