using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanOperator : CompoundStep<bool>
    {
        /// <summary>
        /// The left operand. Will always be evaluated.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Left { get; set; } = null!;


        /// <summary>
        /// The operator to apply.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<BooleanOperator> Operator { get; set; } = null!;


        /// <summary>
        /// The right operand. Will not be evaluated unless necessary.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<bool> Right { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var l = Left.Run(stateMonad);
            if (l.IsFailure) return l;
            var op = Operator.Run(stateMonad);
            if (op.IsFailure) return op.ConvertFailure<bool>();

            switch (op.Value)
            {
                case BooleanOperator.And:
                {
                    if (l.Value == false)
                        return false;

                    var r = Right.Run(stateMonad);
                    return r;
                }
                case BooleanOperator.Or:
                {
                    if (l.Value)
                        return true;

                    var r = Right.Run(stateMonad);
                    return r;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ApplyBooleanStepFactory.Instance;
    }
}
