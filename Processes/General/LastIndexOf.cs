using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class LastIndexOf : CompoundRunnableProcess<int>
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


            return str.Value.LastIndexOf(subString.Value, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => LastIndexOfProcessFactory.Instance;
    }

    /// <summary>
    /// Gets the last instance of substring in a string.
    /// </summary>
    public sealed class LastIndexOfProcessFactory : SimpleRunnableProcessFactory<LastIndexOf, int>
    {
        private LastIndexOfProcessFactory() { }

        public static SimpleRunnableProcessFactory<LastIndexOf, int> Instance { get; } = new LastIndexOfProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Last index of '[{nameof(LastIndexOf.SubString)}]' in '[{nameof(LastIndexOf.String)}]'");
    }
}