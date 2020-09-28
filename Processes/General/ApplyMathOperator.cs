using System;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public sealed class ApplyMathOperator : CompoundStep<int>
    {
        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(StateMonad stateMonad)
        {
            var left = Left.Run(stateMonad);
            if (left.IsFailure) return left;

            var right = Right.Run(stateMonad);
            if (right.IsFailure) return right;

            var @operator = Operator.Run(stateMonad);
            if (@operator.IsFailure) return @operator.ConvertFailure<int>();

            var result = @operator.Value switch
            {
                MathOperator.Add => left.Value + right.Value,
                MathOperator.Subtract => left.Value - right.Value,
                MathOperator.Multiply => left.Value * right.Value,
                MathOperator.Divide => left.Value / right.Value,
                MathOperator.Modulo => left.Value % right.Value,
                MathOperator.Power => Convert.ToInt32(Math.Pow(left.Value, right.Value)),
                _ => throw new ArgumentOutOfRangeException()
            };

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ApplyMathOperatorStepFactory.Instance;


        /// <summary>
        /// The left operand.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Left { get; set; } = null!;

        /// <summary>
        /// The operator to apply.
        /// </summary>
        [StepProperty]
        [Required]

        public IStep<MathOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The right operand.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<int> Right { get; set; } = null!;


    }
}
