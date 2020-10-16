using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Represents an ordered collection of objects.
    /// </summary>
    public sealed class Array<T> : CompoundStep<List<T>>
    {
        /// <inheritdoc />
        public override Result<List<T>, IRunErrors> Run(StateMonad stateMonad)
        {
            var result = Elements.Select(x => x.Run(stateMonad))
                .Combine(RunErrorList.Combine)
                .Map(x => x.ToList());

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayStepFactory.Instance;

        /// <summary>
        /// The elements of this array.
        /// </summary>
        [StepListProperty]
        [Required]
        public IReadOnlyList<IStep<T>> Elements { get; set; } = null!;
    }

    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    public class ArrayStepFactory : GenericStepFactory
    {
        private ArrayStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(Array<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "List<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(List<>), new[] { memberTypeReference });

        /// <inheritdoc />
        public override IStepNameBuilder StepNameBuilder => new StepNameBuilderFromTemplate($"[[{nameof(Array<object>.Elements)}]]");

        /// <inheritdoc />
        protected override Result<ITypeReference> GetMemberType(FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var result =
                freezableStepData.GetListArgument(nameof(Array<object>.Elements))
                    .Bind(x => x.Select(r => r.TryGetOutputTypeReference(typeResolver)).Combine())
                    .Bind(x => MultipleTypeReference.TryCreate(x, TypeName));


            return result;
        }

        /// <summary>
        /// Create a new Freezable Array
        /// </summary>
        public static IFreezableStep CreateFreezable(IEnumerable<IFreezableStep> elements, Configuration? configuration)
        {
            var fpd = new FreezableStepData(new Dictionary<string, StepMember>()
            {
                {nameof(Array<object>.Elements), new StepMember(elements.ToList())}
            });

            return new CompoundFreezableStep(Instance, fpd, configuration);
        }
    }
}