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
    /// Returns success if the Test step returns an error and a failure otherwise.
    /// </summary>
    public sealed class AssertError : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await Test.Run(stateMonad, cancellationToken);

            if (result.IsFailure)
                return Unit.Default;

            return new SingleError("Expected an error but step was successful.", ErrorCode.AssertionFailed, new StepErrorLocation(this));
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
