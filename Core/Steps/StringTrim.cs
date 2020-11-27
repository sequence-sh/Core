using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class StringTrim : CompoundStep<string>
    {

        /// <summary>
        /// The string to change the case of.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The side to trim.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("Both")]
        public IStep<TrimSide> Side { get; set; } = new Constant<TrimSide>(TrimSide.Both);

        /// <inheritdoc />
        public override async Task<Result<string, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await String.Run(stateMonad, cancellationToken).Compose(() => Side.Run(stateMonad, cancellationToken))
                .Map(x => TrimString(x.Item1, x.Item2));
        }

        private static string TrimString(string s, TrimSide side) =>
            side switch
            {
                TrimSide.Start => s.TrimStart(),
                TrimSide.End => s.TrimEnd(),
                TrimSide.Both => s.Trim(),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
            };

        /// <inheritdoc />
        public override IStepFactory StepFactory => StringTrimStepFactory.Instance;
    }


    /// <summary>
    /// Trims a string.
    /// </summary>
    public sealed class StringTrimStepFactory : SimpleStepFactory<StringTrim, string>
    {
        private StringTrimStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringTrim, string> Instance { get; } = new StringTrimStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(TrimSide) };
    }
}