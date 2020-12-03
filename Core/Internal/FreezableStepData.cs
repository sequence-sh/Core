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
    public sealed class FreezableStepData : IEquatable<FreezableStepData>
    {
        /// <summary>
        /// Creates a new FreezableStepData
        /// </summary>
        public FreezableStepData(IReadOnlyDictionary<string, FreezableStepProperty> stepProperties, IErrorLocation location)
        {
            StepProperties = stepProperties;
            Location = location;
        }

        /// <summary>
        /// The step properties.
        /// </summary>
        public IReadOnlyDictionary<string, FreezableStepProperty> StepProperties { get; }

        /// <summary>
        /// The location where this data comes from.
        /// </summary>
        public IErrorLocation Location {get;}

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName, IError> GetVariableName(string name, string typeName) =>
            StepProperties.TryFindOrFail(name,
            ()=> ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x=>x.AsVariableName(name)
                    );

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep, IError> GetStep(string name, string typeName) =>
            StepProperties.TryFindOrFail(name,
            () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Map(x => x.ConvertToStep());


        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> GetStepList(string name, string typeName) =>

            StepProperties.TryFindOrFail(name,
            () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x => x.AsStepList(name)
                    );

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
            return DictionariesEqual1(StepProperties, other.StepProperties);

            static bool DictionariesEqual1(IReadOnlyDictionary<string, FreezableStepProperty> dict1, IReadOnlyDictionary<string, FreezableStepProperty> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));
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

    }
}