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
    /// Negation of a boolean value.
    /// </summary>
    public sealed class Not : CompoundStep<bool>
    {
        /// <inheritdoc />
        public override async Task<Result<bool, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            return await Boolean.Run(stateMonad, cancellationToken).Map(x => !x);
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => NotStepFactory.Instance;

        /// <summary>
        /// The value to negate.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Boolean { get; set; } = null!;
    }

    /// <summary>
    /// Negation of a boolean value.
    /// </summary>
    public class NotStepFactory : SimpleStepFactory<Not, bool>
    {
        private NotStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new NotStepFactory();

        ///// <inheritdoc /> //TODO uncomment
        //public override IStepSerializer Serializer =>
        //    new StepSerializer(TypeName, new FixedStringComponent("not"),
        //    new FixedStringComponent("("),
        //    new StepComponent(nameof(Not.Boolean)),
        //    new FixedStringComponent(")")
        //);



    }
}