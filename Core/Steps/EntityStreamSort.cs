using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Reorder entities according to their property values.
    /// Consumes the stream.
    /// </summary>
    public sealed class EntityStreamSort : CompoundStep<EntityStream>
    {
        /// <inheritdoc />
        public override async Task<Result<EntityStream, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var sortDescending = await Descending.Run(stateMonad, cancellationToken);
            if (sortDescending.IsFailure) return sortDescending.ConvertFailure<EntityStream>();

            var entityStreamResult = await EntityStream.Run(stateMonad, cancellationToken);
            if (entityStreamResult.IsFailure) return entityStreamResult.ConvertFailure<EntityStream>();

            if (stateMonad.VariableExists(VariableName.Entity))
                return new SingleError($"Variable {VariableName.Entity} was already set.", ErrorCode.ReservedVariableName, new StepErrorLocation(this));

            //evaluate the entity stream

            var entitiesResult = await entityStreamResult.Value.TryGetResultsAsync(cancellationToken);

            if (entitiesResult.IsFailure)
                return entitiesResult.ConvertFailure<EntityStream>();


            var list = new List<(string property, Entity entity)>();

            foreach (var entity in entitiesResult.Value)
            {
                var setResult = stateMonad.SetVariable(VariableName.Entity, entity);
                if (setResult.IsFailure) return setResult.ConvertFailure<EntityStream>();
                var propertyValue = await KeySelector.Run(stateMonad, cancellationToken);
                if (propertyValue.IsFailure) return propertyValue.ConvertFailure<EntityStream>();

                list.Add((propertyValue.Value, entity));

            }
            stateMonad.RemoveVariable(VariableName.Entity, false);

            var sortedList =
                sortDescending.Value ?
                    list.OrderByDescending(x => x.property) :
                    list.OrderBy(x => x.property);

            var resultsList = sortedList.Select(x => x.entity).ToList();

            var newStream = Entities.EntityStream.Create(resultsList);

            return newStream;
        }

        /// <summary>
        /// The entities to sort
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <summary>
        /// A function that gets the key to sort by from the variable &lt;Entity&gt;
        /// To sort by multiple properties, concatenate several keys
        /// </summary>
        [StepProperty(2)]
        [Required]
        public IStep<string> KeySelector { get; set; } = null!;

        /// <summary>
        /// Whether to sort in descending order.
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation("False")]
        public IStep<bool> Descending { get; set; } = new Constant<bool>(false);


        /// <inheritdoc />
        public override IStepFactory StepFactory => EntityStreamSortStepFactory.Instance;
    }

    /// <summary>
    /// Reorder entities according to their property values.
    /// Consumes the stream.
    /// </summary>
    public sealed class EntityStreamSortStepFactory : SimpleStepFactory<EntityStreamSort, EntityStream>
    {
        private EntityStreamSortStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<EntityStreamSort, EntityStream> Instance { get; } = new EntityStreamSortStepFactory();
    }

}
