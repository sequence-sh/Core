using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.General
{
    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    public class ArrayStepFactory : GenericStepFactory
    {
        private ArrayStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Array<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new []{memberTypeReference});

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[[{nameof(Array<object>.Elements)}]]");

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData)
        {
            var result =
                freezableStepData.GetListArgument(nameof(Array<object>.Elements))
                    .Bind(x => x.Select(r => r.TryGetOutputTypeReference()).Combine())
                    .Bind(x => MultipleTypeReference.TryCreate(x, TypeName));


            return result;
        }
    }
}