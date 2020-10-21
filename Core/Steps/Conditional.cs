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
    public sealed class Conditional : CompoundStep<Unit>
    {
        /// <inheritdoc />
        public override async Task<Result<Unit, IError>>  Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var result = await Condition.Run(stateMonad, cancellationToken)
                .Bind(r =>
                {
                    if (r)
                        return ThenStep.Run(stateMonad, cancellationToken);

                    if (ElseStep != null)
                        return ElseStep.Run(stateMonad, cancellationToken);
                    else return Task.FromResult(Result.Success<Unit, IError>(Unit.Default));

                });

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ConditionalStepFactory.Instance;

        /// <summary>
        /// Whether to follow the Then Branch
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Condition { get; set; } = null!;

        /// <summary>
        /// The Then Branch.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> ThenStep { get; set; } = null!;

        //TODO else if

        /// <summary>
        /// The Else branch, if it exists.
        /// </summary>
        [StepProperty]
        public IStep<Unit>? ElseStep { get; set; } = null;

    }

    /// <summary>
    /// Executes a statement if a condition is true.
    /// </summary>
    public sealed class ConditionalStepFactory : SimpleStepFactory<Conditional, Unit>
    {
        private ConditionalStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static ConditionalStepFactory Instance { get; } = new ConditionalStepFactory();

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"If [{nameof(Conditional.Condition)}] then [{nameof(Conditional.ThenStep)}] else [{nameof(Conditional.ElseStep)}]");
    }
}
