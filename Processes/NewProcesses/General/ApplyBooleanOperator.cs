using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Processes.NewProcesses.General
{

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanOperator : CompoundRunnableProcess<bool>
    {
        /// <summary>
        /// The left operand. Will always be evaluated.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Left { get; set; }


        /// <summary>
        /// The operator to apply.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<BooleanOperator> Operator { get; set; }


        /// <summary>
        /// The right operand. Will not be evaluated unless necessary.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Right { get; set; }




        /// <inheritdoc />
        public override Result<bool> Run(ProcessState processState)
        {
            var l = Left.Run(processState);
            if (l.IsFailure) return l;
            var op = Operator.Run(processState);
            if (op.IsFailure) return op.ConvertFailure<bool>();

            switch (op.Value)
            {
                case BooleanOperator.And:
                {
                    if (l.Value == false)
                        return false;

                    var r = Right.Run(processState);
                    return r;
                }
                case BooleanOperator.Or:
                {
                    if (l.Value)
                        return true;

                    var r = Right.Run(processState);
                    return r;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override RunnableProcessFactory RunnableProcessFactory => ApplyBooleanProcessFactory.Instance;
    }

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanProcessFactory : SimpleRunnableProcessFactory<ApplyBooleanOperator, bool>
    {
        private ApplyBooleanProcessFactory() { }

        public static RunnableProcessFactory Instance { get; } = new ApplyBooleanProcessFactory();


        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(ApplyBooleanOperator.Left)}] [{nameof(ApplyBooleanOperator.Operator)}] [{nameof(ApplyBooleanOperator.Right)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(BooleanOperator)};
    }

    /// <summary>
    /// A boolean operator.
    /// </summary>
    public enum BooleanOperator
    {
        /// <summary>
        /// Returns true if both left and right are true.
        /// </summary>
        And,
        /// <summary>
        /// Returns true if either left or right is true.
        /// </summary>
        Or
    }







    //public sealed class BreakFromLoop : CompoundRunnableProcess<Unit> //TODO implement BreakFromLoop
    //{
    //}

    //public sealed class ContinueWithLoop : CompoundRunnableProcess<Unit> //TODO implement BreakFromLoop
    //{
    //}


    ////TODO prompt
}
