using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
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
        [Required]
        public IStep<SortOrder> Order { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(StateMonad stateMonad) =>
            Array.Run(stateMonad)
                .Compose(() => Order.Run(stateMonad))
                .Map(x => Sort(x.Item1, x.Item2));

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
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver) =>
            freezableStepData.GetArgument(nameof(SortArray<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }
}