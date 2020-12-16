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
    public sealed class Array<T> : CompoundStep<IAsyncEnumerable<T>>
    {
        /// <inheritdoc />
        public override async Task<Result<IAsyncEnumerable<T>, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var result = await Elements.Select(x => x.Run(stateMonad, cancellationToken))
                .Combine(ErrorList.Combine)
                .Map(x => x.ToList().ToAsyncEnumerable());

            return result;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ArrayStepFactory.Instance;

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
        protected override Result<ITypeReference, IError> GetMemberType(FreezableStepData freezableStepData, TypeResolver typeResolver)
        {
            var result =
                freezableStepData.TryGetStepList(nameof(Array<object>.Elements), StepType)
                    .Bind(x => x.Select(r => r.TryGetOutputTypeReference(typeResolver)).Combine(ErrorList.Combine))
                    .Bind(x => MultipleTypeReference.TryCreate(x, TypeName)
                    .MapError(e=>e.WithLocation(this, freezableStepData)));


            return result;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => ArraySerializer.Instance;

        /// <summary>
        /// Creates an array.
        /// </summary>
        public static Array<T> CreateArray<T>(List<IStep<T>> stepList)
        {
            return new Array<T>()
            {
                Elements = stepList
            };
        }
    }
}