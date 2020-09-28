using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Returns success if the Test step returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertError : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override Result<Unit, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Test.Run(stateMonad);

            if (result.IsFailure)
                return Unit.Default;

            return new RunError("Expected an error but step was successful.", Name, null, ErrorCode.AssertionFailed);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => AssertErrorStepFactory.Instance;

        /// <summary>
        /// The step to test.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Test { get; set; } = null!;
    }
}
