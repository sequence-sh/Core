using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// The data used by a Freezable Step.
    /// </summary>
    public sealed class FreezableStepData : IEquatable<FreezableStepData>
    {
        /// <summary>
        /// Create a new FreezableStepData
        /// </summary>
        public FreezableStepData(IReadOnlyDictionary<string, StepMember> dictionary) => Dictionary =
            dictionary.ToDictionary(x=>x.Key, x=>x.Value, StringComparer.OrdinalIgnoreCase)!;

        /// <summary>
        /// Dictionary mapping property names to step members.
        /// </summary>
        public IReadOnlyDictionary<string, StepMember> Dictionary { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName> GetVariableName(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsVariableName(name));

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep> GetArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsArgument(name));

        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>> GetListArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsListArgument(name));

        /// <inheritdoc />
        public override string ToString()
        {
            return new
            {
                Variables = Dictionary.Count(x => x.Value.MemberType == MemberType.VariableName),
                Processes = Dictionary.Count(x => x.Value.MemberType == MemberType.Step),
                ProcessLists = Dictionary.Count(x => x.Value.MemberType == MemberType.StepList)
            }.ToString()!;
        }

        /// <inheritdoc />
        public bool Equals(FreezableStepData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DictionariesEqual(Dictionary, other.Dictionary);

            static bool DictionariesEqual(IReadOnlyDictionary<string, StepMember> dict1, IReadOnlyDictionary<string, StepMember> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => Dictionary.GetHashCode();

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