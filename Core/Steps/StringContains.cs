using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Returns true if the String contains the substring.
    /// </summary>
    public sealed class StringContains : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var superstringResult = await String.Run(stateMonad, cancellationToken);

            if (superstringResult.IsFailure) return superstringResult.ConvertFailure<bool>();


            var substringResult = await Substring.Run(stateMonad, cancellationToken);

            if (substringResult.IsFailure) return substringResult.ConvertFailure<bool>();

            var ignoreCaseResult = await IgnoreCase.Run(stateMonad, cancellationToken);

            var comparison = ignoreCaseResult.Value ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var r = superstringResult.Value.Contains(substringResult.Value, comparison);

            return r;
        }

        /// <summary>
        /// The superstring
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> String { get; set; } = null!;

        /// <summary>
        /// The substring
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<string> Substring { get; set; } = null!;

        /// <summary>
        /// Whether to ignore case when comparing strings.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("False")]
        public IStep<bool> IgnoreCase { get; set; } = new Constant<bool>(false);



        /// <inheritdoc />
        public override IStepFactory StepFactory => StringContainsFactory.Instance;
    }

    /// <summary>
    /// Returns true if the String contains the substring.
    /// </summary>
    public sealed class StringContainsFactory : SimpleStepFactory<StringContains, bool>
    {
        private StringContainsFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<StringContains, bool> Instance { get; } = new StringContainsFactory();
    }
}
