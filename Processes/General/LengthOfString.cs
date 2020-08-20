using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class LengthOfString : CompoundRunnableProcess<int>
    {
        /// <summary>
        /// The string to measure the length of.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int> Run(ProcessState processState)
        {
            var str = String.Run(processState);
            if (str.IsFailure) return str.ConvertFailure<int>();

            return str.Value.Length;

        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => LengthOfStringProcessFactory.Instance;
    }

    /// <summary>
    /// Calculates the length of the string.
    /// </summary>
    public sealed class LengthOfStringProcessFactory : SimpleRunnableProcessFactory<LengthOfString, int>
    {
        private LengthOfStringProcessFactory() { }

        public static SimpleRunnableProcessFactory<LengthOfString, int> Instance { get; } = new LengthOfStringProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Length of [{nameof(LengthOfString.String)}]");

    }
}