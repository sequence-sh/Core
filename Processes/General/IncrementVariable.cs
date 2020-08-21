using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
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
        public override Result<Unit, IRunErrors> Run(ProcessState processState) =>
            processState.GetVariable<int>(Variable, Name)
                .Compose(() => Amount.Run(processState))
                .Tap(x => processState.SetVariable(Variable, x.Item1 + x.Item2))
                .Map(x => Unit.Default);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => IncrementVariableProcessFactory.Instance;
    }

    /// <summary>
    /// Increment an integer variable by a particular amount
    /// </summary>
    public sealed class IncrementVariableProcessFactory :SimpleRunnableProcessFactory<IncrementVariable, int>
    {
        private IncrementVariableProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<IncrementVariable, int> Instance { get; } = new IncrementVariableProcessFactory();

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>> GetTypeReferencesSet(VariableName variableName, FreezableProcessData freezableProcessData) => Maybe<ITypeReference>.From(new ActualTypeReference(typeof(int)));
    }
}