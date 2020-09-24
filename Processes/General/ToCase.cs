using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
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
        public override Result<string, IRunErrors> Run(ProcessState processState)
        {
            return String.Run(processState).Compose(() => Case.Run(processState))
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
        public override IRunnableProcessFactory RunnableProcessFactory => ToCaseProcessFactory.Instance;
    }

    /// <summary>
    /// Converts a string to a particular case.
    /// </summary>
    public sealed class ToCaseProcessFactory : SimpleRunnableProcessFactory<ToCase, string>
    {
        private ToCaseProcessFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<ToCase, string> Instance { get; } = new ToCaseProcessFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(TextCase)};
    }

    /// <summary>
    /// The case to convert the text to.
    /// </summary>
    public enum TextCase
    {
        /// <summary>
        /// All characters will be in upper case.
        /// </summary>
        Upper,
        /// <summary>
        /// All characters will be in lower case.
        /// </summary>
        Lower,
        /// <summary>
        /// Only the first character in each word will be in upper case.
        /// </summary>
        Title
    }
}