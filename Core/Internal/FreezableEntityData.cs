using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// The data used by a Freezable Step.
    /// </summary>
    public sealed class FreezableEntityData : IEquatable<FreezableEntityData>
    {
        /// <summary>
        /// Creates a new FreezableStepData
        /// </summary>
        public FreezableEntityData(IReadOnlyDictionary<string, FreezableStepProperty> entityProperties, IErrorLocation location)
        {
            EntityProperties = entityProperties;
            Location = location;
        }

        /// <summary>
        /// The step properties.
        /// </summary>
        public IReadOnlyDictionary<string, FreezableStepProperty> EntityProperties { get; }

        /// <summary>
        /// The location where this data comes from.
        /// </summary>
        public IErrorLocation Location { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName, IError> GetVariableName(string name, string typeName) =>
            EntityProperties.TryFindOrFail(name,
                    () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x => x.AsVariableName(name)
                );

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep, IError> GetStep(string name, string typeName) =>
            EntityProperties.TryFindOrFail(name,
                    () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Map(x => x.ConvertToStep());


        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> GetStepList(string name, string typeName) =>

            EntityProperties.TryFindOrFail(name,
                    () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x => x.AsStepList(name)
                );

        /// <inheritdoc />
        public override string ToString()
        {
            var keys = EntityProperties.OrderBy(x => x);
            var keyString = string.Join("; ", keys);

            if (string.IsNullOrWhiteSpace(keyString))
                return "Empty";

            return keyString;
        }

        /// <inheritdoc />
        public bool Equals(FreezableEntityData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            var result = DictionariesEqual1(EntityProperties, other.EntityProperties);

            return result;

            static bool DictionariesEqual1(IReadOnlyDictionary<string, FreezableStepProperty> dict1, IReadOnlyDictionary<string, FreezableStepProperty> dict2)
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
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableEntityData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => EntityProperties.Count;

        /// <summary>
        /// Equals Operator
        /// </summary>
        public static bool operator ==(FreezableEntityData? left, FreezableEntityData? right) => Equals(left, right);

        /// <summary>
        /// Not Equals Operator
        /// </summary>
        public static bool operator !=(FreezableEntityData? left, FreezableEntityData? right) => !Equals(left, right);

        /// <summary>
        /// Gets the variables set by steps in this FreezableStepData.
        /// </summary>
        public Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver)
        {
            var variables = new List<(VariableName variableName, Maybe<ITypeReference>)>();
            var errors = new List<IError>();

            foreach (var stepProperty in EntityProperties)
            {
                stepProperty.Value.Switch(_ => { },
                    LocalGetVariablesSet,
                    l =>
                    {
                        foreach (var step in l) LocalGetVariablesSet(step);
                    }
                );
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError>(ErrorList.Combine(errors));

            return variables;


            void LocalGetVariablesSet(IFreezableStep freezableStep)
            {
                var variablesSet = freezableStep.GetVariablesSet(typeResolver);
                if (variablesSet.IsFailure) errors.Add(variablesSet.Error);
                variables.AddRange(variablesSet.Value);
            }
        }
    }
}