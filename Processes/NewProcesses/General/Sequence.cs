using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// A sequence of steps to be run one after the other.
    /// </summary>
    public sealed class Sequence : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState)
        {
            foreach (var runnableProcess in Steps)
            {
                var r = runnableProcess.Run(processState);
                if (r.IsFailure)
                    return r.ConvertFailure<Unit>();
            }

            return Unit.Default;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => SequenceProcessFactory.Instance;

        /// <summary>
        /// The steps of this sequence.
        /// </summary>
        [RunnableProcessListProperty]
        [Required]
        public IReadOnlyList<IRunnableProcess<Unit>> Steps { get; set; } = null!;


    }


    public sealed class SequenceProcessFactory : RunnableProcessFactory
    {
        private SequenceProcessFactory()
        {
        }

        public static RunnableProcessFactory Instance { get; } = new SequenceProcessFactory();

        /// <inheritdoc />
        public override Result<ITypeReference> TryGetOutputTypeReference(
            IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) =>
            new ActualTypeReference(typeof(Unit));

        /// <inheritdoc />
        public override string TypeName => nameof(Sequence);

        /// <inheritdoc />
        public override string GetProcessName(IReadOnlyDictionary<string, IFreezableProcess> processArguments, IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments)
        {
            var steps = processListArguments.TryFind(nameof(Sequence.Steps))
                .Unwrap(new List<IFreezableProcess>());

            return NameHelper.GetSequenceName(steps);
        }

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => ImmutableArray<Type>.Empty;

        /// <inheritdoc />
        protected override Result<IRunnableProcess> TryCreateInstance(ProcessContext processContext, IReadOnlyDictionary<string, IFreezableProcess> processArguments,
            IReadOnlyDictionary<string, IReadOnlyList<IFreezableProcess>> processListArguments) => new Sequence();
    }
}