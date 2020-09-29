using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// Prints a value to the log.
    /// </summary>
    public sealed class Print<T> : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var r = Value.Run(stateMonad);
            if (r.IsFailure) return r.ConvertFailure<Unit>();

            stateMonad.Logger.LogInformation(r.Value?.ToString());

            return Unit.Default;
        }

        /// <summary>
        /// The Value to Print.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Value { get; set; } = null!;

        /// <inheritdoc />
        public override IStepFactory StepFactory => PrintStepFactory.Instance;
    }
}
