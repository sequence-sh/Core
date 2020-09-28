using System;
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
    public sealed class ToCase : CompoundStep<string>
    {
        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The case to change to.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<TextCase> Case { get; set; } = null!;

        /// <inheritdoc />
        public override Result<string, IRunErrors> Run(StateMonad stateMonad)
        {
            return String.Run(stateMonad).Compose(() => Case.Run(stateMonad))
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
        public override IStepFactory StepFactory => ToCaseStepFactory.Instance;
    }
}