using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Enums;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// Returns the result of the operation.
    /// </summary>
    public sealed class ApplyMathOperator : CompoundStep<int>
    {
        /// <inheritdoc />
        protected override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var left = await Left.Run(stateMonad, cancellationToken);
            if (left.IsFailure) return left;

            var right = await Right.Run(stateMonad, cancellationToken);
            if (right.IsFailure) return right;

            var @operator = await Operator.Run(stateMonad, cancellationToken);
            if (@operator.IsFailure) return @operator.ConvertFailure<int>();

            var result = @operator.Value switch
            {
                MathOperator.Add => left.Value + right.Value,
                MathOperator.Subtract => left.Value - right.Value,
                MathOperator.Multiply => left.Value * right.Value,
                MathOperator.Divide when right.Value == 0 => Result.Failure<int, IError>(
                    new SingleError(new StepErrorLocation(this), ErrorCode.DivideByZero)),
                MathOperator.Divide => left.Value / right.Value,
                MathOperator.Modulo => left.Value % right.Value,
                MathOperator.Power  => Convert.ToInt32(Math.Pow(left.Value, right.Value)),
                _                   => new SingleError(new StepErrorLocation(this), ErrorCode.UnexpectedEnumValue, nameof(Operator), @operator.Value)
            };

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ApplyMathOperatorStepFactory.Instance;


        /// <summary>
        /// The left operand.
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<int> Left { get; set; } = null!;

        /// <summary>
        /// The operator to apply.
        /// </summary>
        [StepProperty(2)]
        [Required]

        public IStep<MathOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The right operand.
        /// </summary>
        [StepProperty(3)]
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
        public override IStepSerializer Serializer => new StepSerializer(TypeName,
            new StepComponent(nameof(ApplyMathOperator.Left)),
            SpaceComponent.Instance,
            new EnumDisplayComponent<MathOperator>(nameof(ApplyMathOperator.Operator)),
            SpaceComponent.Instance,
            new StepComponent(nameof(ApplyMathOperator.Right))
        );
    }

}
