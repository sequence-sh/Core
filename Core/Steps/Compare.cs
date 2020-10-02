using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class Compare<T> : CompoundStep<bool> where T : IComparable
    {
        /// <summary>
        /// The item to the left of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Left { get; set; } = null!;

        /// <summary>
        /// The operator to use for comparison.
        /// </summary>
        [StepProperty]
        [Required]

        public IStep<CompareOperator> Operator { get; set; } = null!;

        /// <summary>
        /// The item to the right of the operator.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<T> Right { get; set; } = null!;


        /// <inheritdoc />
        public override Result<bool, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Left.Run(stateMonad)
                .Compose(() => Operator.Run(stateMonad), () => Right.Run(stateMonad))
                .Bind(x => CompareItems(x.Item1, x.Item2, x.Item3));


            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => CompareStepFactory.Instance;

        private static Result<bool, IRunErrors> CompareItems(T item1, CompareOperator compareOperator, T item2)
        {
            return compareOperator switch
            {
                CompareOperator.Equals => item1.Equals(item2),
                CompareOperator.NotEquals => !item1.Equals(item2),
                CompareOperator.LessThan => item1.CompareTo(item2) < 0,
                CompareOperator.LessThanOrEqual => item1.CompareTo(item2) <= 0,
                CompareOperator.GreaterThan => item1.CompareTo(item2) > 0,
                CompareOperator.GreaterThanOrEqual => item1.CompareTo(item2) >= 0,
                _ => throw new ArgumentOutOfRangeException(nameof(compareOperator), compareOperator, null)
            };
        }

    }

    /// <summary>
    /// Compares two items.
    /// </summary>
    public sealed class CompareStepFactory : GenericStepFactory
    {
        private CompareStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static StepFactory Instance { get; } = new CompareStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Compare<>);

        /// <inheritdoc />
        public override IEnumerable<Type> EnumTypes => new[] { typeof(CompareOperator) };

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var result = freezableStepData.GetArgument(nameof(Compare<int>.Left))
                .Bind(x => x.TryGetOutputTypeReference(typeResolver))
                .Compose(() => freezableStepData.GetArgument(nameof(Compare<int>.Right))
                    .Bind(x => x.TryGetOutputTypeReference(typeResolver)))
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName));

            return result;
        }

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(Compare<int>.Left)}] [{nameof(Compare<int>.Operator)}] [{nameof(Compare<int>.Right)}]");


        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new FixedStringComponent("("),
            new IntegerComponent(nameof(Compare<int>.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<CompareOperator>(nameof(Compare<int>.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(Compare<int>.Right)),
            new FixedStringComponent(")")
        );
    }
}