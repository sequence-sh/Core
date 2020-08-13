using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{
    /// <summary>
    /// Converts a string to a particular case.
    /// </summary>
    public sealed class ToCase : CompoundRunnableProcess<string>
    {
        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<string> String { get; set; } = null!;

        /// <summary>
        /// The case to change to.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<TextCase> Case { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string> Run(ProcessState processState)
        {
            return  String.Run(processState).Compose(() => Case.Run(processState))
                .Map(x => Convert(x.Item1, x.Item2));
        }

        private static string Convert(string s, TextCase textCase) =>
            textCase switch
            {
                TextCase.Upper => s.ToUpperInvariant(),
                TextCase.Lower => s.ToLowerInvariant(),
                TextCase.Title => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s),
                _ => throw new ArgumentOutOfRangeException(nameof(textCase), textCase, null)
            };

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ToCaseProcessFactory.Instance;
    }

    public sealed class ToCaseProcessFactory : SimpleRunnableProcessFactory<ToCase, string>
    {
        private ToCaseProcessFactory() { }

        public static SimpleRunnableProcessFactory<ToCase, string> Instance { get; } = new ToCaseProcessFactory();
    }

    /// <summary>
    /// The case to convert the text to.
    /// </summary>
    public enum TextCase
    {
        Upper,
        Lower,
        Title
    }
}