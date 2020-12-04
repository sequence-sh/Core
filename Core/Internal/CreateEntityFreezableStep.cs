using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Freezes into a create entity step
    /// </summary>
    public class CreateEntityFreezableStep : IFreezableStep
    {
        /// <summary>
        /// Create a new CreateEntityFreezableStep
        /// </summary>
        /// <param name="data"></param>
        public CreateEntityFreezableStep(FreezableStepData data) => FreezableStepData = data;

        /// <summary>
        /// The data
        /// </summary>
        public FreezableStepData FreezableStepData { get; }

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other) => other is CreateEntityFreezableStep oStep && FreezableStepData.Equals(oStep.FreezableStepData);

        /// <inheritdoc />
        public string StepName => "Create Entity";

        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {

            var results = new List<Result<(string name, IStep value), IError>>();



            foreach (var (propertyName, stepMember) in FreezableStepData.StepProperties)
            {
                var frozen = stepMember.ConvertToStep()
                    .TryFreeze(stepContext)
                    .Map(s=> (propertyName, s));

                results.Add(frozen);
            }

            var r =

            results.Combine(ErrorList.Combine)
                .Map(v=>
                v.ToDictionary(x=>x.name, x=>x.value));


            if (r.IsFailure) return r.ConvertFailure<IStep>();


            return new CreateEntityStep(r.Value);
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            return FreezableStepData.GetVariablesSet(typeResolver);
        }

        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => new ActualTypeReference(typeof(Entities.Entity));
    }
}