using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public sealed class Not : CompoundRunnableProcess<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState) => Boolean.Run(processState).Map(x => !x);

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => NotProcessFactory.Instance;

        /// <summary>
        /// The value to negate.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Boolean { get; set; }
    }

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public class NotProcessFactory : SimpleRunnableProcessFactory<Not, bool>
    {
        private NotProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RunnableProcessFactory Instance { get; } = new NotProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Not [{nameof(Not.Boolean)}]");

        /// <inheritdoc />
        public override IProcessSerializer Serializer { get; } = new ProcessSerializer(
            new FixedStringComponent("not"),
            new SpaceComponent(true),
            new BooleanComponent(nameof(Not.Boolean)));
    }
}