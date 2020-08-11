using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Returns one result if a condition is true and another if the condition is false.
    /// </summary>
    public sealed class TestSequenceFactory : RunnableProcessFactory
    {
        private TestSequenceFactory() { }

        public static RunnableProcessFactory Instance { get; } = new TestSequenceFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var result =
            processArguments.TryFindOrFail(nameof(Test<object>.ThenValue), "Test Then not set.")
                .Compose(() => processArguments.TryFindOrFail(nameof(Test<object>.ElseValue), "Test Else not set."))
                .Bind(x => x.Item1.TryGetOutputTypeReference().Compose(() => x.Item2.TryGetOutputTypeReference()))
                .Bind(x => MultipleTypeReference.TryCreate(new[] {x.Item1, x.Item2}, TypeName));

            return result;
        }

        /// <inheritdoc />
        public override string TypeName => FormatTypeName(typeof(Test<>));

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;


        /// <inheritdoc />
        public override ProcessNameBuilder ProcessNameBuilder { get; } = new ProcessNameBuilder($"'[{nameof(Test<object>.Condition)}]' then '[{nameof(Test<object>.ThenValue)}]' else '[{nameof(Test<object>.ElseValue)}]'");

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            TryGetOutputTypeReference(processArguments, processListArguments)
                .Bind(processContext.TryGetTypeFromReference)
                .Bind(x => TryCreateGeneric(typeof(Test<object>), x));



        /// <summary>
        /// Returns one result if a condition is true and another if the condition is false.
        /// </summary>
        public sealed class Test<T> : CompoundRunnableProcess<T>
        {
            /// <inheritdoc />
            public override Result<T> Run(ProcessState processState)
            {
                var result = Condition.Run(processState)
                    .Bind(r => r ? ThenValue.Run(processState) : ElseValue.Run(processState));

                return result;
            }

            /// <inheritdoc />
            public override RunnableProcessFactory RunnableProcessFactory => TestSequenceFactory.Instance;


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
    }
}