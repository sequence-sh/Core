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
        public override RunnableProcessFactory RunnableProcessFactory => NotProcessFactory.Instance;

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

        public static RunnableProcessFactory Instance { get; } = new NotProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Not [{nameof(Not.Boolean)}]");

        /// <inheritdoc />
        public override Maybe<ICustomSerializer> CustomSerializer => Maybe<ICustomSerializer>.From(new CustomSerializer(
            new FixedStringComponent("not"),
            new SpaceComponent(true),
            new BooleanComponent(nameof(Not.Boolean))
            ));
    }
}