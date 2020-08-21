using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Reorder an array.
    /// </summary>
    public sealed class SortArray<T> : CompoundRunnableProcess<List<T>>
    {
        /// <summary>
        /// The array to modify.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<T>> Array { get; set; } = null!;

        /// <summary>
        /// The order to use.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<SortOrder> Order { get; set; } = null!;

        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(ProcessState processState) =>
            Array.Run(processState)
                .Compose(() => Order.Run(processState))
                .Map(x => Sort(x.Item1, x.Item2));

        private static List<T> Sort(IEnumerable<T> list, SortOrder sortOrder) =>
            sortOrder switch
            {
                SortOrder.Ascending => list.OrderBy(x => x).ToList(),
                SortOrder.Descending => list.OrderByDescending(x => x).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(sortOrder), sortOrder, null)
            };

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => SortArrayProcessFactory.Instance;
    }

    /// <summary>
    /// Reorder an array.
    /// </summary>

    public sealed class SortArrayProcessFactory : GenericProcessFactory
    {
        private SortArrayProcessFactory() { }

        public static GenericProcessFactory Instance { get; } = new SortArrayProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(SortArray<>);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(SortOrder)};


        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(SortArray<object>.Array))
                .Bind(x => x.TryGetOutputTypeReference())
                .BindCast<ITypeReference, GenericTypeReference>()
                .Map(x => x.ChildTypes)
                .BindSingle();
    }

    /// <summary>
    /// The direction to sort by.
    /// </summary>
    public enum SortOrder
    {
        Ascending,
        Descending
    }
}