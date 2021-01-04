using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    /// Represents an ordered collection of objects.
    /// </summary>
    [Alias("Array")]
    [Alias("NewArray")]
    public sealed class ArrayNew<T> : CompoundStep<Array<T>>
    {
        /// <inheritdoc />
        public override async Task<Result<Array<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var result = await Elements.Select(x => x.Run(stateMonad, cancellationToken))
                .Combine(ErrorList.Combine)
                .Map(x => x.ToList().ToSequence());

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayNewStepFactory.Instance;

        /// <inheritdoc />
        public override bool ShouldBracketWhenSerialized => false;

        /// <summary>
        /// The elements of the array.
        /// </summary>
        [StepListProperty(1)]
        [Required]
        public IReadOnlyList<IStep<T>> Elements { get; set; } = null!;
    }

    /// <summary>
    /// The factory for creating Arrays.
    /// </summary>
    public class ArrayNewStepFactory : GenericStepFactory
    {
        private ArrayNewStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new ArrayNewStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayNew<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array<T>";

        /// <inheritdoc />
        protected override ITypeReference GetOutputTypeReference(ITypeReference memberTypeReference) => new GenericTypeReference(typeof(Core.Array<>), new[] { memberTypeReference });

        /// <inheritdoc />
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData, TypeResolver typeResolver)
        {
            var result =
                freezableStepData.TryGetStepList(nameof(ArrayNew<object>.Elements), StepType)
                    .Bind(x => x.Select(r => r.TryGetOutputTypeReference(typeResolver)).Combine(ErrorList.Combine))
                    .Bind(x => MultipleTypeReference.TryCreate(x, TypeName)
                    .MapError(e=>e.WithLocation( freezableStepData)));


            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArraySerializer.Instance;

        /// <summary>
        /// Creates an array.
        /// </summary>
        public static ArrayNew<T> CreateArray<T>(List<IStep<T>> stepList)
        {
            return new()
            {
                Elements = stepList
            };
        }
    }
}