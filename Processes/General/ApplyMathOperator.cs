using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public sealed class ApplyMathOperator : CompoundRunnableProcess<int>
    {
        /// <inheritdoc />
        public override Result<int, IRunErrors> Run(ProcessState processState)
        {
            var left = Left.Run(processState);
            if (left.IsFailure) return left;

            var right = Right.Run(processState);
            if (right.IsFailure) return right;

            var @operator = Operator.Run(processState);
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
        public override IRunnableProcessFactory RunnableProcessFactory => ApplyMathOperatorProcessFactory.Instance;


        /// <summary>
        /// The left operand.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Left { get; set; } = null!;

        /// <summary>
        /// The operator to apply.
        /// </summary>
        [RunnableProcessProperty]
        [Required]

        public IRunnableProcess<MathOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The right operand.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<int> Right { get; set; } = null!;


    }

    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public class ApplyMathOperatorProcessFactory : SimpleRunnableProcessFactory<ApplyMathOperator, int>
    {
        private ApplyMathOperatorProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleRunnableProcessFactory<ApplyMathOperator, int> Instance { get; } = new ApplyMathOperatorProcessFactory();

        ///// <inheritdoc />
        //protected override string ProcessNameTemplate => $"[{nameof(ApplyMathOperator.Left)}] [{nameof(ApplyMathOperator.Operator)}] [{nameof(ApplyMathOperator.Right)}]";

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(MathOperator)};

        /// <inheritdoc />
        public override IProcessSerializer Serializer { get; } = new ProcessSerializer(
            new IntegerComponent(nameof(ApplyMathOperator.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<MathOperator>(nameof(ApplyMathOperator.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(ApplyMathOperator.Right))
        );
    }

    /// <summary>
    /// An operator that can be applied to two numbers.
    /// </summary>
    public enum MathOperator
    {
        /// <summary>
        /// Sentinel value
        /// </summary>
        None,

        /// <summary>
        /// Add the left and right operands.
        /// </summary>
        [Display(Name = "+")]
        Add,
        /// <summary>
        /// Subtract the right operand from the left.
        /// </summary>
        [Display(Name = "-")]
        Subtract,
        /// <summary>
        /// Multiply the left and right operands.
        /// </summary>
        [Display(Name = "*")]
        Multiply,
        /// <summary>
        /// Divide the left operand by the right.
        /// </summary>
        [Display(Name = "/")]
        Divide,
        /// <summary>
        /// Reduce the left operand modulo the right.
        /// </summary>
        [Display(Name = "%")]
        Modulo,
        /// <summary>
        /// Raise the left operand to the power of the right.
        /// </summary>
        [Display(Name = "^")]
        Power

    }
}
