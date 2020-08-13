using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Returns true if either operand is true
    /// </summary>
    public sealed class Or : CompoundRunnableProcess<bool>
    {
        /// <summary>
        /// The left operand. Will always be evaluated.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Left { get; set; } = null!;


        /// <summary>
        /// The right operand. Will not be evaluated unless the left operand is false.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Right { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool> Run(ProcessState processState) => Left.Run(processState).Bind(x => x ? true : Right.Run(processState));

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => OrProcessFactory.Instance;
    }

    /// <summary>
    /// Returns true if either operand is true
    /// </summary>
    public sealed class OrProcessFactory : SimpleRunnableProcessFactory<Or, bool>
    {
        private OrProcessFactory() { }

        public static SimpleRunnableProcessFactory<Or, bool> Instance { get; } = new OrProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(Or.Left)}] || [{nameof(Or.Right)}]");

    }
}