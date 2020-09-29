using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;

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
        public Result<IStep> TryFreeze(StepContext stepContext) => StepFactory.TryFreeze(stepContext, FreezableStepData, StepConfiguration);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<(VariableName VariableName, ITypeReference type)>> TryGetVariablesSet
        {
            get
            {
                var result = FreezableStepData
                    .Dictionary.Values
                    .Select(TryGetStepMemberVariablesSet)
                    .Combine()
                    .Map(x=>x.SelectMany(y=>y).ToList() as IReadOnlyCollection<(VariableName name, ITypeReference type)>);

                return result;


                 Result<IReadOnlyCollection<(VariableName, ITypeReference)>> TryGetStepMemberVariablesSet(StepMember stepMember) =>
                     stepMember.Join(vn =>
                             StepFactory.GetTypeReferencesSet(vn, FreezableStepData)
                                 .Map(y=> y.Map(x => new[] { (vn, x) } as IReadOnlyCollection<(VariableName, ITypeReference)>)
                                 .Unwrap(ImmutableArray<(VariableName, ITypeReference)>.Empty)),
                         y => y.TryGetVariablesSet,
                         y => y.Select(z => z.TryGetVariablesSet).Combine().Map(x =>
                             x.SelectMany(q => q).ToList() as IReadOnlyCollection<(VariableName, ITypeReference)>));
            }
        }



        /// <inheritdoc />
        public string StepName => StepFactory.StepNameBuilder.GetFromArguments(FreezableStepData);

        /// <inheritdoc />
        public Result<ITypeReference> TryGetOutputTypeReference() => StepFactory.TryGetOutputTypeReference(FreezableStepData);

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