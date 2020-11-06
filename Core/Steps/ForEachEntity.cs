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
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<Unit> Action { get; set; } = null!;


        /// <summary>
        /// The name of the variable to loop over.
        /// </summary>
        [VariableName]
        [Required]
        public VariableName VariableName { get; set; }

        /// <summary>
        /// The entities to iterate over.
        /// </summary>
        [StepProperty]
        [Required]
        public IStep<EntityStream> EntityStream { get; set; } = null!;

        /// <inheritdoc />
        public override async Task<Result<Unit, IError>> Run(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var records = await EntityStream.Run(stateMonad, cancellationToken);
            if (records.IsFailure) return records.ConvertFailure<Unit>();

            async Task RunAction(Entity record)
            {
                var setResult = stateMonad.SetVariable(VariableName, record);

                if (setResult.IsFailure)
                    throw new ErrorException(setResult.Error);

                var result = await Action.Run(stateMonad, cancellationToken);

                if (result.IsFailure)
                    throw new ErrorException(result.Error);
            }

            var r = await records.Value.Act(RunAction, new StepErrorLocation(this));

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
        public override Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName, FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            Maybe<ITypeReference>.From(new ActualTypeReference(typeof(Entity)));
    }

}