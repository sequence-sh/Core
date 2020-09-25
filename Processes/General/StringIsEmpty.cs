using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmpty : CompoundRunnableProcess<bool>
    {
        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState)
        {
            var str = String.Run(processState);
            if (str.IsFailure) return str.ConvertFailure<bool>();

            return string.IsNullOrWhiteSpace(str.Value);
        }

        /// <summary>
        /// The string to check for being empty.
        /// </summary>
        [RunnableProcessPropertyAttribute]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;


        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => StringIsEmptyProcessFactory.Instance;
    }

    /// <summary>
    /// Returns whether a string is empty.
    /// </summary>
    public sealed class StringIsEmptyProcessFactory : SimpleRunnableProcessFactory<StringIsEmpty, bool>
    {
        private StringIsEmptyProcessFactory() { }

        public static SimpleRunnableProcessFactory<StringIsEmpty, bool> Instance { get; } = new StringIsEmptyProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"'[{nameof(LengthOfString.String)}]' is empty?");
    }
}