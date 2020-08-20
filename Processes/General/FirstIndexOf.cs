using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the first instance of substring in a string.
    /// </summary>
    public sealed class FirstIndexOf : CompoundRunnableProcess<int>
    {
        /// <summary>
        /// The string to check.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <summary>
        /// The substring to find.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> SubString { get; set; } = null!;

        /// <inheritdoc />
        public override Result<int> Run(ProcessState processState)
        {
            var str = String.Run(processState);
            if (str.IsFailure) return str.ConvertFailure<int>();

            var subString = SubString.Run(processState);
            if (subString.IsFailure) return subString.ConvertFailure<int>();


            return str.Value.IndexOf(subString.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => FirstIndexOfProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the first instance of substring in a string.
    /// </summary>
    public sealed class FirstIndexOfProcessFactory : SimpleRunnableProcessFactory<FirstIndexOf, int>
    {
        private FirstIndexOfProcessFactory() { }

        public static SimpleRunnableProcessFactory<FirstIndexOf, int> Instance { get; } = new FirstIndexOfProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"First index of '[{nameof(FirstIndexOf.SubString)}]' in '[{nameof(FirstIndexOf.String)}]'");
    }
}