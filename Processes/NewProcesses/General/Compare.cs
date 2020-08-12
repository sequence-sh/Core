using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class CompareProcessFactory : RunnableProcessFactory
    {
        private CompareProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new CompareProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(FreezableProcessData freezableProcessData) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        public override Type ProcessType => typeof(Compare<>);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes =>new[]{typeof(CompareOperator)};

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, FreezableProcessData freezableProcessData)
        {
            return TryGetMemberTypeReference(freezableProcessData )
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(outputType => TryCreateGeneric(typeof(Compare<>), outputType));
        }

        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilder($"[{nameof(Compare<int>.Left)}] [{nameof(Compare<int>.Operator)}] [{nameof(Compare<int>.Right)}]");

        private Result<ITypeReference> TryGetMemberTypeReference(FreezableProcessData freezableProcessData)
        {
            var result = freezableProcessData.GetArgument(nameof(Compare<int>.Left))
                .Bind(x => x.TryGetOutputTypeReference())
                .Compose(() => freezableProcessData.GetArgument(nameof(Compare<int>.Right))
                    .Bind(x => x.TryGetOutputTypeReference()))
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName));

            return result;
        }

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
            public override Result<bool> Run(ProcessState processState)
            {
                var result = Left.Run(processState).Compose(() => Operator.Run(processState), () => Right.Run(processState))
                    .Bind(x => CompareItems(x.Item1, x.Item2, x.Item3));


                return result;
            }

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => CompareProcessFactory.Instance;

            private static Result<bool> CompareItems(T item1, CompareOperator compareOperator, T item2)
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
    }
}