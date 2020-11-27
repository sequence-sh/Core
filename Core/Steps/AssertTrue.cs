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
    /// Returns an error if the nested step does not return true.
    /// </summary>
    public sealed class AssertTrue : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {

            return await Bool.Run(stateMonad, cancellationToken).Ensure(x => x,
                    new SingleError($"Assertion Failed '{Bool.Name}'", ErrorCode.IndexOutOfBounds, new StepErrorLocation(this)))
                .Map(x => Unit.Default);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => AssertTrueStepFactory.Instance;

        /// <summary>
        /// The bool to test.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Bool { get; set; } = null!;
    }

    /// <summary>
    /// Returns an error if the nested step does not return true.
    /// </summary>
    public sealed class AssertTrueStepFactory : SimpleStepFactory<AssertTrue, Unit>
    {
        private AssertTrueStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<AssertTrue, Unit> Instance { get; } = new AssertTrueStepFactory();
    }
}
