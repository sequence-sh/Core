using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Processes.General
{
    /// <summary>
    /// Applies a mathematical operator to two integers.
    /// </summary>
    public sealed class ApplyMathOperator : CompoundRunnableProcess<int>
    {
        /// <inheritdoc />
        public override Result<int> Run(ProcessState processState)
        {
            var left = Left.Run(processState);
            if (left.IsFailure) return left;

            var right = Right.Run(processState);
            if (right.IsFailure) return right;

            var @operator = Operator.Run(processState);
            if (@operator.IsFailure) return @operator.ConvertFailure<int>();

            var result = @operator.Value switch
            {
                MathOperator.Plus => left.Value + right.Value,
                MathOperator.Minus => left.Value - right.Value,
                MathOperator.Times => left.Value * right.Value,
                MathOperator.Divide => left.Value / right.Value,
                MathOperator.Modulo => left.Value % right.Value,
                MathOperator.ToThePowerOf => left.Value ^ right.Value,
                _ => throw new ArgumentOutOfRangeException()
            };

            return result;
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ApplyMathOperatorProcessFactory.Instance;


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

        public static SimpleRunnableProcessFactory<ApplyMathOperator, int> Instance { get; } = new ApplyMathOperatorProcessFactory();

        ///// <inheritdoc />
        //protected override string ProcessNameTemplate => $"[{nameof(ApplyMathOperator.Left)}] [{nameof(ApplyMathOperator.Operator)}] [{nameof(ApplyMathOperator.Right)}]";

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(MathOperator)};
    }


    public enum MathOperator
    {
        Plus,
        Minus,
        Times,
        Divide,
        Modulo,
        ToThePowerOf

    }
}
