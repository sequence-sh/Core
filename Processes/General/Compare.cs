using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class Compare<T> : CompoundRunnableProcess<bool> where T : IComparable
    {
        /// <summary>
        /// The item to the left of the operator.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Left { get; set; } = null!;

        /// <summary>
        /// The operator to use for comparison.
        /// </summary>
        [RunnableProcessProperty]
        [Required]

        public IRunnableProcess<CompareOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The item to the right of the operator.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Right { get; set; } = null!;


        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState)
        {
            var result = Left.Run(processState)
                .Compose(() => Operator.Run(processState), () => Right.Run(processState))
                .Bind(x => CompareItems(x.Item1, x.Item2, x.Item3));


            return result;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => CompareProcessFactory.Instance;

        private static Result<bool, IRunErrors> CompareItems(T item1, CompareOperator compareOperator, T item2)
        {
            return compareOperator switch
            {
                CompareOperator.Equals => item1.Equals(item2),
                CompareOperator.NotEquals => !item1.Equals(item2),
                CompareOperator.LessThan => item1.CompareTo(item2) < 0,
                CompareOperator.LessThanOrEqual => item1.CompareTo(item2) <= 0,
                CompareOperator.GreaterThan => item1.CompareTo(item2) > 0,
                CompareOperator.GreaterThanOrEqual => item1.CompareTo(item2) >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(compareOperator), compareOperator, null)
            };
        }

    }

    /// <summary>
    /// An operator to use for comparison.
    /// </summary>
    public enum CompareOperator
    {
        /// <summary>
        /// Sentinel value.
        /// </summary>
        None,
        #pragma warning disable 1591
        [Display(Name = "==")]
        Equals,

        [Display(Name = "!=")]
        NotEquals,
        [Display(Name = "<")]
        LessThan,
        [Display(Name = "<=")]
        LessThanOrEqual,
        [Display(Name = ">")]
        GreaterThan,
        [Display(Name = ">=")]
        GreaterThanOrEqual
#pragma warning restore 1591
    }

    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class CompareProcessFactory : GenericProcessFactory
    {
        private CompareProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RunnableProcessFactory Instance { get; } = new CompareProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Compare<>);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(CompareOperator)};

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData)
        {
            var result = freezableProcessData.GetArgument(nameof(Compare<int>.Left))
                .Bind(x => x.TryGetOutputTypeReference())
                .Compose(() => freezableProcessData.GetArgument(nameof(Compare<int>.Right))
                    .Bind(x => x.TryGetOutputTypeReference()))
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName));

            return result;
        }

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(Compare<int>.Left)}] [{nameof(Compare<int>.Operator)}] [{nameof(Compare<int>.Right)}]");


        /// <inheritdoc />
        public override IProcessSerializer Serializer { get; } = new ProcessSerializer(
            new IntegerComponent(nameof(Compare<int>.Left)),
            new SpaceComponent(false),
            new EnumDisplayComponent<CompareOperator>(nameof(Compare<int>.Operator)),
            new SpaceComponent(false),
            new IntegerComponent(nameof(Compare<int>.Right))
        );
    }

}