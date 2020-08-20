using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStrings : CompoundRunnableProcess<string>
    {
        /// <summary>
        /// The delimiter to use.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> Delimiter { get; set; } = null!;

        /// <summary>
        /// The elements to iterate over.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<List<string>> List { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(ProcessState processState)
        {
            var list = List.Run(processState);
            if (list.IsFailure) return list.ConvertFailure<string>();

            var delimiter = Delimiter.Run(processState);
            if (delimiter.IsFailure) return delimiter;


            var result = string.Join(delimiter.Value, list.Value);

            return result;

        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => JoinStringsProcessFactory.Instance;
    }

    /// <summary>
    /// Join strings with a delimiter.
    /// </summary>
    public sealed class JoinStringsProcessFactory : SimpleRunnableProcessFactory<JoinStrings, string>
    {
        private JoinStringsProcessFactory() { }

        public static SimpleRunnableProcessFactory<JoinStrings, string> Instance { get; } = new JoinStringsProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"Join [{nameof(JoinStrings.List)}]");
    }
}