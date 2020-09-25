using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
{
    /// <summary>
    /// The data used by a Freezable Process.
    /// </summary>
    public sealed class FreezableProcessData : IEquatable<FreezableProcessData>
    {
        /// <summary>
        /// Create a new FreezableProcessData
        /// </summary>
        public FreezableProcessData(IReadOnlyDictionary<string, ProcessMember> dictionary) => Dictionary =
            dictionary.ToDictionary(x=>x.Key, x=>x.Value, StringComparer.OrdinalIgnoreCase)!;

        /// <summary>
        /// Dictionary mapping property names to process members.
        /// </summary>
        public IReadOnlyDictionary<string, ProcessMember> Dictionary { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName> GetVariableName(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsVariableName(name));

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableProcess> GetArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsArgument(name));

        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableProcess>> GetListArgument(string name) =>
            Dictionary.TryFindOrFail(name, null).Bind(x => x.AsListArgument(name));

        /// <inheritdoc />
        public override string ToString()
        {
            return new
            {
                Variables = Dictionary.Count(x => x.Value.MemberType == MemberType.VariableName),
                Processes = Dictionary.Count(x => x.Value.MemberType == MemberType.Process),
                ProcessLists = Dictionary.Count(x => x.Value.MemberType == MemberType.ProcessList)
            }.ToString()!;
        }

        /// <inheritdoc />
        public bool Equals(FreezableProcessData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DictionariesEqual(Dictionary, other.Dictionary);

            static bool DictionariesEqual(IReadOnlyDictionary<string, ProcessMember> dict1, IReadOnlyDictionary<string, ProcessMember> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableProcessData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => Dictionary.GetHashCode();

        /// <summary>
        /// Equals Operator
        /// </summary>
        public static bool operator ==(FreezableProcessData? left, FreezableProcessData? right) => Equals(left, right);

        /// <summary>
        /// Not Equals Operator
        /// </summary>
        public static bool operator !=(FreezableProcessData? left, FreezableProcessData? right) => !Equals(left, right);
    }
}