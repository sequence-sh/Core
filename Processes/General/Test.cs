using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class Test<T> : CompoundRunnableProcess<T>
    {
        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(ProcessState processState)
        {
            var result = Condition.Run(processState)
                .Bind(r => r ? ThenValue.Run(processState) : ElseValue.Run(processState));

            return result;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => TestProcessFactory.Instance;


        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<T> ThenValue { get; set; } = null!;

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<T> ElseValue { get; set; } = null!;
    }

    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class TestProcessFactory : GenericProcessFactory
    {
        private TestProcessFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericProcessFactory Instance { get; } = new TestProcessFactory();

        /// <inheritdoc />
        public override Type ProcessType => typeof(Test<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableProcessData freezableProcessData) =>
            freezableProcessData.GetArgument(nameof(Test<object>.ThenValue))
                .Compose(() => freezableProcessData.GetArgument(nameof(Test<object>.ElseValue)))
                .Bind(x => x.Item1.TryGetOutputTypeReference().Compose(() => x.Item2.TryGetOutputTypeReference()))
                .Bind(x => MultipleTypeReference.TryCreate(new[] { x.Item1, x.Item2 }, TypeName));
    }
}