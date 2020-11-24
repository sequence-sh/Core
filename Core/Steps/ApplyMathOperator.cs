using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
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
        public override async Task<Result<int, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
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
                    new SingleError("Divide by Zero Error", ErrorCode.DivideByZero, new StepErrorLocation(this))),
                MathOperator.Divide => left.Value / right.Value,
                MathOperator.Modulo => left.Value % right.Value,
                MathOperator.Power => Convert.ToInt32(Math.Pow(left.Value, right.Value)),
                _ => new SingleError($"Could not apply '{@operator.Value}'", ErrorCode.UnexpectedEnumValue, new StepErrorLocation(this))
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


        /// <summary>
        /// Create a freezable ApplyMathOperator step.
        /// </summary>
        public static IFreezableStep CreateFreezable(IFreezableStep left, IFreezableStep compareOperator, IFreezableStep right)
        {
            var dict = new Dictionary<string, IFreezableStep>
            {
                {nameof(ApplyMathOperator.Left), left},
                {nameof(ApplyMathOperator.Operator), compareOperator},
                {nameof(ApplyMathOperator.Right), right},
            };

            var fpd = new FreezableStepData(dict, null, null);
            var step = new CompoundFreezableStep(Instance, fpd, null);

            return step;
        }
    }

}
