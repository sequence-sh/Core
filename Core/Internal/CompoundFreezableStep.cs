using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableStep : IFreezableStep, IEquatable<CompoundFreezableStep>
    {
        /// <summary>
        /// Creates a new CompoundFreezableStep.
        /// </summary>
        public CompoundFreezableStep(IStepFactory stepFactory, FreezableStepData freezableStepData, Configuration? stepConfiguration)
        {
            StepFactory = stepFactory;
            FreezableStepData = freezableStepData;
            StepConfiguration = stepConfiguration;
        }


        /// <summary>
        /// The factory for this step.
        /// </summary>
        public IStepFactory StepFactory { get; }

        /// <summary>
        /// The data for this step.
        /// </summary>
        public FreezableStepData FreezableStepData { get; }

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        public Configuration? StepConfiguration { get; }


        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext) =>
            StepFactory.TryFreeze(stepContext, FreezableStepData, StepConfiguration);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference typeReference)>, IError> TryGetVariablesSet(TypeResolver typeResolver)
        {
            var result = FreezableStepData
                .VariableNameDictionary.Values.Select(TryGetVariableNameVariablesSet)
                .Concat(FreezableStepData.StepDictionary.Values.Select(TryGetStepVariablesSet))
                .Concat(FreezableStepData.StepListDictionary.Values.Select(TryGetStepListVariablesSet))
                    .Combine(ErrorList.Combine)
                    .Map(x => x.SelectMany(y => y).ToList() as IReadOnlyCollection<(VariableName name, ITypeReference type)>);



            return result;


            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetVariableNameVariablesSet(VariableName vn) =>

                StepFactory.GetTypeReferencesSet(vn, FreezableStepData, typeResolver)
                    .Map(y => y.Map(x => new[] {(vn, x)} as IReadOnlyCollection<(VariableName, ITypeReference)>)
                        .Unwrap(ImmutableArray<(VariableName, ITypeReference)>.Empty));

            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetStepVariablesSet(IFreezableStep y) => y.TryGetVariablesSet(typeResolver);

            Result<IReadOnlyCollection<(VariableName, ITypeReference)>, IError> TryGetStepListVariablesSet(IReadOnlyList<IFreezableStep> y) =>

                y.Select(z => z.TryGetVariablesSet(typeResolver)).Combine(ErrorList.Combine).Map(x =>
                    x.SelectMany(q => q).ToList() as IReadOnlyCollection<(VariableName, ITypeReference)>);
        }



        /// <inheritdoc />
        public string StepName => StepFactory.StepNameBuilder.GetFromArguments(FreezableStepData);

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => StepFactory.TryGetOutputTypeReference(FreezableStepData, typeResolver);

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(CompoundFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return StepFactory.Equals(other.StepFactory) &&
                   FreezableStepData.Equals(other.FreezableStepData) &&
                   Equals(StepConfiguration, other.StepConfiguration);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is CompoundFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(StepFactory, FreezableStepData, StepConfiguration);

        /// <summary>
        /// Equals Operator.
        /// </summary>
        public static bool operator ==(CompoundFreezableStep? left, CompoundFreezableStep? right) => Equals(left, right);

        /// <summary>
        /// Not Equals Operator.
        /// </summary>
        public static bool operator !=(CompoundFreezableStep? left, CompoundFreezableStep? right) => !Equals(left, right);
    }
}