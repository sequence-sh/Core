using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class And : CompoundRunnableProcess<bool>
    {
        /// <summary>
        /// The left operand. Will always be evaluated.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Left { get; set; }


        /// <summary>
        /// The right operand. Will not be evaluated unless the left operand is true.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Right { get; set; }

        /// <inheritdoc />
        public override Result<bool> Run(ProcessState processState) => Left.Run(processState).Bind(x => x ? Right.Run(processState) : false);

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => AndProcessFactory.Instance;
    }

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class AndProcessFactory : SimpleRunnableProcessFactory<And, bool>
    {
        private AndProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new AndProcessFactory();


        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(And.Left)}] && [{nameof(And.Right)}]");
    }








    //public sealed class BreakFromLoop : CompoundRunnableProcess<Unit> //TODO implement BreakFromLoop
    //{
    //}

    //public sealed class ContinueWithLoop : CompoundRunnableProcess<Unit> //TODO implement BreakFromLoop
    //{
    //}


    ////TODO prompt
}
