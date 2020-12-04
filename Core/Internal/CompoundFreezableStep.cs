using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step that is not a constant or a variable reference.
    /// </summary>
    public sealed class CompoundFreezableStep : IFreezableStep
    {
        /// <summary>
        /// Creates a new CompoundFreezableStep.
        /// </summary>
        public CompoundFreezableStep(string stepName, FreezableStepData freezableStepData, Configuration? stepConfiguration)
        {
            StepName = stepName;
            FreezableStepData = freezableStepData;
            StepConfiguration = stepConfiguration;
        }


        /// <summary>
        /// The data for this step.
        /// </summary>
        public FreezableStepData FreezableStepData { get; }

        /// <summary>
        /// Configuration for this step.
        /// </summary>
        public Configuration? StepConfiguration { get; }

        /// <summary>
        /// Try to get this step factory from the store.
        /// </summary>
        public Result<IStepFactory, IError> TryGetStepFactory(StepFactoryStore stepFactoryStore)
        {
            var r=
            stepFactoryStore.Dictionary.TryFindOrFail(StepName,
                () => ErrorHelper.MissingStepError(StepName).WithLocation(FreezableStepData.Location)
            );

            return r;
        }


        /// <inheritdoc />
        public string StepName { get; }

        /// <inheritdoc />
        public Result<IStep, IError> TryFreeze(StepContext stepContext)
        {
            return TryGetStepFactory(stepContext.TypeResolver.StepFactoryStore).Bind(x =>
                x.TryFreeze(stepContext, FreezableStepData, StepConfiguration));
        }

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            var stepFactory = TryGetStepFactory(typeResolver.StepFactoryStore);

            if (stepFactory.IsFailure) return stepFactory.ConvertFailure<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>>();

            var dataResult = FreezableStepData.GetVariablesSet(typeResolver);
            if (dataResult.IsFailure) return dataResult;

            return dataResult.Value.Concat(stepFactory.Value.GetTypeReferencesSet(FreezableStepData, typeResolver))
                .ToList();
        }

        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver)
        {
            return TryGetStepFactory(typeResolver.StepFactoryStore).Bind(x => x.TryGetOutputTypeReference(FreezableStepData, typeResolver));
        }

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is CompoundFreezableStep fs)
            {
                return StepName.Equals(fs.StepName, StringComparison.OrdinalIgnoreCase) &&
                   FreezableStepData.Equals(fs.FreezableStepData) &&
                   Equals(StepConfiguration, fs.StepConfiguration);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(StepName, FreezableStepData, StepConfiguration);
    }
}