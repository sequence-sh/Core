using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    public class DoNothing : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            return Unit.Default;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => DoNothingStepFactory.Instance;
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public class DoNothingStepFactory : SimpleStepFactory<DoNothing, Unit>
    {
        private DoNothingStepFactory() { }

        /// <summary>
        /// This instance.
        /// </summary>
        public static SimpleStepFactory<DoNothing, Unit> Instance { get; } = new DoNothingStepFactory();
    }
}
