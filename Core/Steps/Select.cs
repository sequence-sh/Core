using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Apply a function to every entity in an entity stream.
    /// </summary>
    public sealed class Select : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();


            var currentState = stateMonad.GetState().ToImmutableDictionary();

            async ValueTask<Entity> SelectAction(Entity record)
            {
                var scopedMonad = new ScopedStateMonad(stateMonad, currentState,
                    new KeyValuePair<VariableName, object>(VariableName.Entity, record));

                var result = await Selector.Run(scopedMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);

                return result.Value;
            }

            var newStream = entityStreamResult.Value.Apply(SelectAction);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(Order = 1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function to get the selected entity, using the variable &lt;Entity&gt;
        /// </summary>
        [StepProperty(Order = 2)]
        [Required]
        public IStep<Entity> Selector { get; set; } = null!;


        /// <inheritdoc />
        public override IStepFactory StepFactory => SelectStepFactory.Instance;
    }

    /// <summary>
    /// Apply a function to every entity in an entity stream.
    /// </summary>
    public sealed class SelectStepFactory : SimpleStepFactory<Select, EntityStream>
    {
        private SelectStepFactory() {}

        /// <inheritdoc />
        public override IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet
        {
            get
            {
                yield return (VariableName.Entity, new ActualTypeReference(typeof(Entity)));
            }
        }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<Select, EntityStream> Instance { get; } = new SelectStepFactory();
    }
}