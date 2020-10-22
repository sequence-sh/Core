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
    public sealed class SortArray<T> : CompoundStep<List<T>>
    {
        /// <summary>
        /// The array to modify.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The order to use.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("Ascending")]
        public IStep<SortOrder> Order { get; set; } = new Constant<SortOrder>(SortOrder.Ascending);

        /// <inheritdoc />
        public override async Task<Result<List<T>, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            return await Array.Run(stateMonad, cancellationToken)
                .Compose(() => Order.Run(stateMonad, cancellationToken))
                .Map(x => Sort(x.Item1, x.Item2));
        }

        private static List<T> Sort(IEnumerable<T> list, SortOrder sortOrder) =>
            sortOrder switch
            {
                SortOrder.Ascending => list.OrderBy(x => x).ToList(),
                SortOrder.Descending => list.OrderByDescending(x => x).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
            };

        /// <inheritdoc />
        public override IStepFactory StepFactory => SortArrayStepFactory.Instance;
    }


    /// <summary>
    /// Reorder an array.
    /// </summary>
    public sealed class SortArrayStepFactory : GenericStepFactory
    {
        private SortArrayStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new SortArrayStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(SortArray<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new[] { memberTypeReference });

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(SortOrder) };

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";


        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(SortArray<object>.Array), TypeName)
                .MapError(e=>e.WithLocation(this, freezableStepData))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Bind(x => x.TryGetGenericTypeReference(typeResolver, 0)
                    .MapError(e=>e.WithLocation(this, freezableStepData)))
                .Map(x => x as ITypeReference)
        ;
    }
}