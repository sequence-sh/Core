using System.Collections.Generic;
using System.Collections.Immutable;
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
        public CreateEntityFreezableStep(FreezableStepData data) => Data = data;

        /// <summary>
        /// The data
        /// </summary>
        public FreezableStepData Data { get; }

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other) => other is CreateEntityFreezableStep oStep && Data.Equals(oStep.Data);

        /// <inheritdoc />
        public string StepName => "Create Entity";

        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {

            var results = new List<Result<(string name, IStep value), IError>>();



            foreach (var (propertyName, stepMember) in Data.StepProperties)
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
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(TypeResolver typeResolver)
        {
            var result = Data
                .StepProperties.Values
                .Select(x =>
                    x.Match(TryGetVariableNameVariablesSet, TryGetStepVariablesSet, TryGetStepListVariablesSet))
                .Combine(ErrorList.Combine)
                .Map(x => x.SelectMany(y => y)
                    .ToList() as IReadOnlyCollection<(VariableName name, ITypeReference type)>);



            return result;


            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetVariableNameVariablesSet(
                VariableName vn) =>
                ImmutableArray<(VariableName, ITypeReference)>.Empty;

            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetStepVariablesSet(IFreezableStep y) => y.TryGetVariablesSet(typeResolver);

            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetStepListVariablesSet(IReadOnlyList<IFreezableStep> y) =>

                y.Select(z => z.TryGetVariablesSet(typeResolver)).Combine(ErrorList.Combine).Map(x =>
                    x.SelectMany(q => q).ToList() as IReadOnlyCollection<(VariableName, ITypeReference)>);
        }

        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => new ActualTypeReference(typeof(Entity));
    }
}