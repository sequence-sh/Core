using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariable : CompoundRunnableProcess<Unit>
    {
        /// <summary>
        /// The variable to increment.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName Variable { get; set; }

        /// <summary>
        /// The amount to increment by.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Amount { get; set; } = null!;

        /// <inheritdoc />
        public override Result<Unit> Run(ProcessState processState) =>
            processState.GetVariable<int>(Variable)
                .Compose(() => Amount.Run(processState))
                .Tap(x => processState.SetVariable(Variable, x.Item1 + x.Item2))
                .Map(x => Unit.Default);

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => IncrementVariableProcessFactory.Instance;
    }

    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariableProcessFactory :SimpleRunnableProcessFactory<IncrementVariable, int>
    {
        private IncrementVariableProcessFactory() { }

        public static SimpleRunnableProcessFactory<IncrementVariable, int> Instance { get; } = new IncrementVariableProcessFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}