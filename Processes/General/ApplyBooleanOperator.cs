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
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanOperator : CompoundRunnableProcess<bool>
    {
        /// <summary>
        /// The left operand. Will always be evaluated.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Left { get; set; } = null!;


        /// <summary>
        /// The operator to apply.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<BooleanOperator> Operator { get; set; } = null!;


        /// <summary>
        /// The right operand. Will not be evaluated unless necessary.
        /// </summary>
        [RunnableProcessProperty]
        [Required]
        public IRunnableProcess<bool> Right { get; set; } = null!;

        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(ProcessState processState)
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
        public override IRunnableProcessFactory RunnableProcessFactory => ApplyBooleanProcessFactory.Instance;
    }

    /// <summary>
    /// Returns true if both operands are true
    /// </summary>
    public sealed class ApplyBooleanProcessFactory : SimpleRunnableProcessFactory<ApplyBooleanOperator, bool>
    {
        private ApplyBooleanProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RunnableProcessFactory Instance { get; } = new ApplyBooleanProcessFactory();

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate($"[{nameof(ApplyBooleanOperator.Left)}] [{nameof(ApplyBooleanOperator.Operator)}] [{nameof(ApplyBooleanOperator.Right)}]");

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] {typeof(BooleanOperator)};

        /// <inheritdoc />
        public override Maybe<ICustomSerializer> CustomSerializer { get; } = Maybe<ICustomSerializer>.From(new CustomSerializer(
                new BooleanComponent(nameof(ApplyBooleanOperator.Left)),
                new SpaceComponent(false),
                new EnumDisplayComponent<BooleanOperator>(nameof(ApplyBooleanOperator.Operator)),
                new SpaceComponent(false),
                new BooleanComponent(nameof(ApplyBooleanOperator.Right))
                ));
    }

    /// <summary>
    /// A boolean operator.
    /// </summary>
    public enum BooleanOperator
    {
        /// <summary>
        /// Returns true if both left and right are true.
        /// </summary>
        [Display(Name = "and")]
        And,
        /// <summary>
        /// Returns true if either left or right is true.
        /// </summary>
        [Display(Name = "or")]
        Or
    }

}
