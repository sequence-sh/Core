using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        public IRunnableProcess<CompareOperator> Operator { get; set; }= null!;

        /// <summary>
        /// The item to the right of the operator.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> Right { get; set; }= null!;


        /// <inheritdoc />
        public override Result<bool> Run(ProcessState processState)
        {
            var result = Left.Run(processState).Compose(()=> Operator.Run(processState), ()=> Right.Run(processState))
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

    public sealed class CompareProcessFactory : RunnableProcessFactory
    {
        private CompareProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new CompareProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            return new ActualTypeReference(typeof(bool));
        }

        /// <inheritdoc />
        public override string TypeName => FormatTypeName(typeof(Compare<>));

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes =>new[]{typeof(CompareOperator)};

        /// <inheritdoc />
        public override string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var leftName = processArguments.TryFind(nameof(Compare<int>.Left)).Unwrap(NameHelper.MissingProcess.Instance);
            var operatorName = processArguments.TryFind(nameof(Compare<int>.Operator)).Unwrap(NameHelper.MissingProcess.Instance);
            var rightName = processArguments.TryFind(nameof(Compare<int>.Right)).Unwrap(NameHelper.MissingProcess.Instance);


            return NameHelper.GetCompareName(leftName, operatorName, rightName);
        }

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            TryGetMemberTypeReference(processArguments, processListArguments)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(outputType => TryCreateGeneric(typeof(Compare<>), outputType));


        private Result<ITypeReference> TryGetMemberTypeReference(
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var result = processArguments.TryFindOrFail(nameof(Compare<int>.Left), "Compare Left is not set.")
                .Bind(x => x.TryGetOutputTypeReference())
                .Compose(() => processArguments.TryFindOrFail(nameof(Compare<int>.Right), "Compare Right is not set.")
                    .Bind(x => x.TryGetOutputTypeReference()))
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName));

            return result;
        }
    }
}