using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class Conditional : CompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(ProcessState processState)
        {
            var result = Condition.Run(processState)
                .Bind(r =>
                {
                    if (r)
                        return ThenProcess.Run(processState);
                    return ElseProcess?.Run(processState) ?? Unit.Default;
                });

            return result;
        }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => ConditionalProcessFactory.Instance;

        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<Unit> ThenProcess { get; set; } = null!;

        //TODO else if
        //public IReadOnlyList<IRunnableProcess<Unit>> ElseIfProcesses

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        public IRunnableProcess<Unit>? ElseProcess { get; set; } = null;

    }

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class ConditionalProcessFactory : SimpleRunnableProcessFactory<Conditional, Unit>
    {
        private ConditionalProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static ConditionalProcessFactory Instance { get; } = new ConditionalProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"If [{nameof(Conditional.Condition)}] then [{nameof(Conditional.ThenProcess)}] else [{nameof(Conditional.ElseProcess)}]");
    }
}
