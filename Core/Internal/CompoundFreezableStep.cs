﻿using System;
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
    public sealed class CompoundFreezableStep : IFreezableStep
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
        public string StepName => StepFactory.StepNameBuilder.GetFromArguments(FreezableStepData, StepFactory);

        /// <param name="typeResolver"></param>
        /// <inheritdoc />
        public Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver) => StepFactory.TryGetOutputTypeReference(FreezableStepData, typeResolver);

        /// <inheritdoc />
        public override string ToString() => StepName;

        /// <inheritdoc />
        public bool Equals(IFreezableStep? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            if (other is CompoundFreezableStep fs)
            {
                return StepFactory.Equals(fs.StepFactory) &&
                   FreezableStepData.Equals(fs.FreezableStepData) &&
                   Equals(StepConfiguration, fs.StepConfiguration);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is IFreezableStep other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(StepFactory, FreezableStepData, StepConfiguration);
    }
}