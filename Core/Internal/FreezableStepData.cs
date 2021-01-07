using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using StepParameterDict = System.Collections.Generic.IReadOnlyDictionary<Reductech.EDR.Core.Internal.StepParameterReference, Reductech.EDR.Core.Internal.FreezableStepProperty>;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// The data used by a Freezable Step.
    /// </summary>
    public sealed class FreezableStepData : IEquatable<FreezableStepData>
    {
        /// <summary>
        /// Creates a new FreezableStepData
        /// </summary>
        public FreezableStepData(StepParameterDict stepProperties, IErrorLocation location)
        {
            StepProperties = stepProperties;
            Location = location;
        }

        /// <summary>
        /// The step properties.
        /// </summary>
        public StepParameterDict StepProperties { get; }

        /// <summary>
        /// The location where this data comes from.
        /// </summary>
        public IErrorLocation Location {get;}



        private Result<T, IError> TryGetValue<T>(string propertyName, Type stepType,
            Func<FreezableStepProperty, Result<T, IError>> extractValue)
        {
            var property = stepType.GetProperty(propertyName);

            if (property == null) throw new Exception($"{stepType.Name} does not have property {propertyName}");

            foreach (var reference in StepParameterReference.GetPossibleReferences(property))
                if (StepProperties.TryGetValue(reference, out var value))
                    return extractValue(value);

            return Result.Failure<T, IError>(ErrorHelper.MissingParameterError(propertyName).WithLocation(Location));
        }


        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName, IError> TryGetVariableName(string propertyName, Type stepType) => TryGetValue(propertyName, stepType, x =>
            x.AsVariableName(propertyName));


        /// <summary>
        /// Gets a step argument
        /// </summary>
        public Result<IFreezableStep, IError> TryGetStep(string propertyName, Type stepType) => TryGetValue(propertyName, stepType, x =>
            Result.Success<IFreezableStep, IError>(x.ConvertToStep()));

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> TryGetStepList(string propertyName, Type stepType) =>
            TryGetValue(propertyName, stepType, x => x.AsStepList(propertyName));


        /// <inheritdoc />
        public override string ToString()
        {
            var keys = StepProperties.OrderBy(x=>x);
            var keyString = string.Join("; ", keys);

            if (string.IsNullOrWhiteSpace(keyString))
                return "Empty";

            return keyString;
        }

        /// <inheritdoc />
        public bool Equals(FreezableStepData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            var result = DictionariesEqual1(StepProperties, other.StepProperties);

            return result;

            static bool DictionariesEqual1(StepParameterDict dict1, StepParameterDict dict2)
            {
                if (dict1.Count != dict2.Count) return false;
                foreach (var key in dict1.Keys)
                {
                    if (!dict2.ContainsKey(key)) return false;
                    if (!dict1[key].Equals(dict2[key])) return false;
                }

                return true;
            }
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => StepProperties.Count;

        /// <summary>
        /// Equals Operator
        /// </summary>
        public static bool operator ==(FreezableStepData? left, FreezableStepData? right) => Equals(left, right);

        /// <summary>
        /// Not Equals Operator
        /// </summary>
        public static bool operator !=(FreezableStepData? left, FreezableStepData? right) => !Equals(left, right);

        /// <summary>
        /// Gets the variables set by steps in this FreezableStepData.
        /// </summary>
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(string stepName, TypeResolver typeResolver)
        {
            var variables = new List<(VariableName variableName, Maybe<ITypeReference>)>();
            var errors = new List<IError>();

            foreach (var (key, freezableStepProperty) in StepProperties)
            {
                if (!typeResolver.StepFactoryStore.IsScopedFunction(stepName, key))
                    freezableStepProperty.Switch(_ => { },
                        LocalGetVariablesSet,
                        l =>
                        {
                            foreach (var step in l)
                                LocalGetVariablesSet(step);
                        });
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError>(ErrorList.Combine(errors));

            return variables;


            void LocalGetVariablesSet(IFreezableStep freezableStep)
            {
                var variablesSet = freezableStep.GetVariablesSet(typeResolver);
                if (variablesSet.IsFailure)
                    errors.Add(variablesSet.Error);
                else
                    variables.AddRange(variablesSet.Value);
            }
        }
    }
}