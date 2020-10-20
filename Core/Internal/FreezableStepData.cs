using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// The data used by a Freezable Step.
    /// </summary>
    public sealed class FreezableStepData : IEquatable<FreezableStepData>
    {
        /// <summary>
        /// Try to create a new FreezableStepData
        /// </summary>
        public static Result<FreezableStepData> TryCreate(IStepFactory stepFactory, IReadOnlyDictionary<string, StepMember> dictionary)
        {
            var stepDictionary = new Dictionary<string, IFreezableStep>();
            var variableNameDictionary = new Dictionary<string, VariableName>();
            var stepListDictionary = new Dictionary<string, IReadOnlyList<IFreezableStep>>();

            var errors = new List<Result>();

            foreach (var (key, value) in dictionary)
            {
                var mt = stepFactory.GetExpectedMemberType(key);

                if(mt == MemberType.NotAMember)
                    errors.Add(Result.Failure($"{stepFactory.StepType} does not have a property '{key}'"));
                else
                {
                    var convertedMember = value.TryConvert(mt, false);

                    if(convertedMember.IsFailure)
                        errors.Add(convertedMember);
                    else
                    {
                        convertedMember.Value
                            .Match(
                            x => variableNameDictionary.Add(key, x),
                            x => stepDictionary.Add(key, x),
                            x => stepListDictionary.Add(key, x)
                            );
                    }
                }
            }

            if (errors.Any())
                return errors.Combine().ConvertFailure<FreezableStepData>();

            return new FreezableStepData(stepDictionary, variableNameDictionary, stepListDictionary);
        }

        /// <summary>
        /// Creates a new FreezableStepData
        /// </summary>
        public FreezableStepData(
            IEnumerable<KeyValuePair<string, IFreezableStep>>? steps,
            IEnumerable<KeyValuePair<string, VariableName>>? variableNames,
            IEnumerable<KeyValuePair<string, IReadOnlyList<IFreezableStep>>>? stepLists)
        {
            StepDictionary = (steps ?? Enumerable.Empty<KeyValuePair<string, IFreezableStep>>()).ToDictionary(x=>x.Key, x=>x.Value, StringComparer.OrdinalIgnoreCase)!;
            VariableNameDictionary = (variableNames ?? Enumerable.Empty<KeyValuePair<string, VariableName>>()).ToDictionary(x=>x.Key, x=>x.Value, StringComparer.OrdinalIgnoreCase)!;
            StepListDictionary = (stepLists ?? Enumerable.Empty<KeyValuePair<string, IReadOnlyList<IFreezableStep>>>()).ToDictionary(x=>x.Key, x=>x.Value, StringComparer.OrdinalIgnoreCase)!;

            StepMembersDictionary = StepDictionary.Select(x => new KeyValuePair<string, StepMember>(x.Key, new StepMember(x.Value)))
                        .Concat(VariableNameDictionary
                            .Select(x => new KeyValuePair<string, StepMember>(x.Key, new StepMember(x.Value))))
                            .Concat(StepListDictionary.Select(x =>
                                new KeyValuePair<string, StepMember>(x.Key, new StepMember(x.Value))))
                        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)!;
        }

        /// <summary>
        /// Dictionary mapping property names to step members.
        /// </summary>
        public IReadOnlyDictionary<string, IFreezableStep> StepDictionary { get; }


        /// <summary>
        /// Dictionary mapping property names to variable names.
        /// </summary>
        public IReadOnlyDictionary<string, VariableName> VariableNameDictionary { get; }


        /// <summary>
        /// Dictionary mapping property names to lists of step members
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<IFreezableStep>> StepListDictionary { get; }


        /// <summary>
        /// Step Members by key
        /// </summary>
        public IReadOnlyDictionary<string, StepMember> StepMembersDictionary { get; }

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName> GetVariableName(string name) => VariableNameDictionary.TryFindOrFail(name, null);

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep> GetArgument(string name) => StepDictionary.TryFindOrFail(name, null);

        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>> GetListArgument(string name) => StepListDictionary.TryFindOrFail(name, null);

        /// <inheritdoc />
        public override string ToString()
        {
            var keys = StepDictionary.Keys.Concat(StepListDictionary.Keys).Concat(VariableNameDictionary.Keys).OrderBy(x=>x);
            var keyString = string.Join("; ", keys);

            return keyString;
        }

        /// <inheritdoc />
        public bool Equals(FreezableStepData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DictionariesEqual(VariableNameDictionary, other.VariableNameDictionary) &&
                DictionariesEqual(StepDictionary, other.StepDictionary) &&
                DictionariesEqual(StepListDictionary, other.StepListDictionary);

            static bool DictionariesEqual<T>(IReadOnlyDictionary<string, T> dict1, IReadOnlyDictionary<string, T> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key]!.Equals(dict2[key]));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(StepDictionary.Count, VariableNameDictionary.Count, StepListDictionary.Count);

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