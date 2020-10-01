using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// Returns the result of the operation.
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

    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public class ApplyMathOperatorStepFactory : SimpleStepFactory<ApplyMathOperator, int>
    {
        private ApplyMathOperatorStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ApplyMathOperator, int> Instance { get; } = new ApplyMathOperatorStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(MathOperator) };

        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new FixedStringComponent("("),
            new IntegerComponent(nameof(ApplyMathOperator.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<MathOperator>(nameof(ApplyMathOperator.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(ApplyMathOperator.Right)),
            new FixedStringComponent(")")
        );
    }

}
