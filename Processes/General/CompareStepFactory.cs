using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.General
{
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
        public override IEnumerable<Type> EnumTypes => new[] {typeof(CompareOperator)};

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Boolean);

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new ActualTypeReference(typeof(bool));

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData)
        {
            var result = freezableStepData.GetArgument(nameof(Compare<int>.Left))
                .Bind(x => x.TryGetOutputTypeReference())
                .Compose(() => freezableStepData.GetArgument(nameof(Compare<int>.Right))
                    .Bind(x => x.TryGetOutputTypeReference()))
                .Map(x => new[] { x.Item1, x.Item2 })
                .Bind((x) => MultipleTypeReference.TryCreate(x, TypeName));

            return result;
        }

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[{nameof(Compare<int>.Left)}] [{nameof(Compare<int>.Operator)}] [{nameof(Compare<int>.Right)}]");


        /// <inheritdoc />
        public override IStepSerializer Serializer { get; } = new StepSerializer(
            new IntegerComponent(nameof(Compare<int>.Left)),
            new SpaceComponent(),
            new EnumDisplayComponent<CompareOperator>(nameof(Compare<int>.Operator)),
            new SpaceComponent(),
            new IntegerComponent(nameof(Compare<int>.Right))
        );
    }
}