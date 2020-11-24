using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Steps
{
    /// <summary>
    /// Perform an action on each entity in the stream.
    /// </summary>
    public sealed class ForEachEntity : CompoundStep<Unit>
    {
        /// <summary>
        /// The action to perform repeatedly.
        /// Use the Variable &lt;Entity&gt; to access the entity.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;




        /// <summary>
        /// The entities to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(IStateMonad stateMonad,
            CancellationToken cancellationToken)
        {
            var entities = await EntityStream.Run(stateMonad, cancellationToken);
            if (entities.IsFailure) return entities.ConvertFailure<Unit>();

            if(stateMonad.VariableExists(VariableName.Entity))
                return new SingleError($"Variable {VariableName.Entity} was already set.", ErrorCode.ReservedVariableName, new StepErrorLocation(this));

            async Task RunAction(Entity record)
            {
                var setResult = stateMonad.SetVariable(VariableName.Entity, record);

                if (setResult.IsFailure)
                    throw new ErrorException(setResult.Error);

                var result = await Action.Run(stateMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);
            }

            var r = await entities.Value.Act(RunAction, new StepErrorLocation(this));

            stateMonad.RemoveVariable(VariableName.Entity, false);

            return r;
        }

        /// <inheritdoc />
        public override IStepFactory StepFactory => ForEachEntityStepFactory.Instance;
    }


    /// <summary>
    /// Perform an action on each record in the stream.
    /// </summary>
    public sealed class ForEachEntityStepFactory : SimpleStepFactory<ForEachEntity, Unit>
    {
        private ForEachEntityStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<ForEachEntity, Unit> Instance { get; } = new ForEachEntityStepFactory();

        /// <inheritdoc />
        public override IEnumerable<(VariableName VariableName, ITypeReference typeReference)> FixedVariablesSet
        {
            get
            {
                yield return (VariableName.Entity, new ActualTypeReference(typeof(Entity)));
            }
        }

        /// <inheritdoc />
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            Maybe<ITypeReference>.From(new ActualTypeReference(typeof(Entity)));
    }

}