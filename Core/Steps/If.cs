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
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class If : CompoundStep<Unit>
    {
        /// <inheritdoc />
        protected override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var result = await Condition.Run(stateMonad, cancellationToken)
                .Bind(r =>
                {
                    if (r)
                        return Then.Run(stateMonad, cancellationToken);

                    if (Else != null)
                        return Else.Run(stateMonad, cancellationToken);
                    else return Task.FromResult(Result.Success<Unit, IError>(Unit.Default));

                });

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => IfStepFactory.Instance;

        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<Unit> Then { get; set; } = null!;

        //TODO else if

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("Do Nothing")]
        public IStep<Unit>? Else { get; set; } = null;

    }

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class IfStepFactory : SimpleStepFactory<If, Unit>
    {
        private IfStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IfStepFactory Instance { get; } = new IfStepFactory();
    }
}
