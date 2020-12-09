using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Converts a string to a particular case.
    /// </summary>
    public sealed class StringToCase : CompoundStep<string>
    {
        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The case to change to.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<TextCase> Case { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await String.Run(stateMonad, cancellationToken).Compose(() => Case.Run(stateMonad, cancellationToken))
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
        public override IStepFactory StepFactory => StringToCaseStepFactory.Instance;
    }

    /// <summary>
    /// Converts a string to a particular case.
    /// </summary>
    public sealed class StringToCaseStepFactory : SimpleStepFactory<StringToCase, string>
    {
        private StringToCaseStepFactory() { }
        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringToCase, string> Instance { get; } = new StringToCaseStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(TextCase) };
    }
}