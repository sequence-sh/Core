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
    /// Returns true if the SuperString contains the substring.
    /// </summary>
    public sealed class DoesStringContain : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var superstringResult = await Superstring.Run(stateMonad, cancellationToken);

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
        public IStep<string> Superstring { get; set; } = null!;

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
        public override IStepFactory StepFactory => DoesStringContainFactory.Instance;
    }

    /// <summary>
    /// Returns true if the SuperString contains the substring.
    /// </summary>
    public sealed class DoesStringContainFactory : SimpleStepFactory<DoesStringContain, bool>
    {
        private DoesStringContainFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<DoesStringContain, bool> Instance { get; } = new DoesStringContainFactory();
    }
}
