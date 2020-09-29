using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.General
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

    /// <summary>
    /// Returns success if the Test step returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertErrorStepFactory : SimpleStepFactory<AssertError, Unit>
    {
        private AssertErrorStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AssertError, Unit> Instance { get; } = new AssertErrorStepFactory();
    }
}
