using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Try to create a new FreezableStepData
        /// </summary>
        public static Result<FreezableStepData, IErrorBuilder> TryCreate(IStepFactory stepFactory, IReadOnlyDictionary<string, StepMember> dictionary)
        {
            var stepDictionary = new Dictionary<string, IFreezableStep>();
            var variableNameDictionary = new Dictionary<string, VariableName>();
            var stepListDictionary = new Dictionary<string, IReadOnlyList<IFreezableStep>>();

            var remainingRequiredProperties = stepFactory.RequiredProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var errors = new List<IErrorBuilder>();

            foreach (var (key, value) in dictionary)
            {
                var (memberType, type) = stepFactory.GetExpectedMemberType(key);

                if (memberType == MemberType.NotAMember)
                    errors.Add(ErrorHelper.UnexpectedParameterError(key, stepFactory.TypeName));
                else
                {
                    var convertedMember = value.TryConvert(memberType, false);

                    if(convertedMember.IsFailure)
                        errors.Add(convertedMember.Error);
                    else
                    {
                        remainingRequiredProperties.Remove(key);

                        var addResult =
                        convertedMember.Value
                            .Join(
                            x => AddVariableName(variableNameDictionary, key, x),
                            x => TryAddStep(stepDictionary, key, x, type),
                            x => TryAddStepList(stepListDictionary, key, x, type)
                            );
                        if(addResult.IsFailure)
                            errors.Add(addResult.Error);
                    }
                }
            }

            foreach (var remainingRequiredProperty in remainingRequiredProperties)
                errors.Add(ErrorHelper.MissingParameterError(remainingRequiredProperty, stepFactory.TypeName));

            if (errors.Any())
                return Result.Failure<FreezableStepData, IErrorBuilder>( ErrorBuilderList.Combine(errors));

            return new FreezableStepData(stepDictionary, variableNameDictionary, stepListDictionary);
        }

        private static Result<Unit, IErrorBuilder> AddVariableName(IDictionary<string, VariableName> variableNames, string key, VariableName variableName)
        {
            variableNames.Add(key, variableName);
            return Unit.Default;
        }

        private static Result<Unit, IErrorBuilder> TryAddStep(IDictionary<string, IFreezableStep> freezableSteps, string key, IFreezableStep freezableStep, Type? expectedType)
        {
            var convertedResult = TryConvertStep(freezableStep, expectedType);
            if (convertedResult.IsFailure)
                return convertedResult.ConvertFailure<Unit>();

            freezableSteps.Add(key, convertedResult.Value);

            return Unit.Default;
        }

        private static Result<Unit, IErrorBuilder> TryAddStepList(IDictionary<string, IReadOnlyList<IFreezableStep>> stepListDictionary, string key, IReadOnlyList<IFreezableStep> stepList, Type? expectedType)
        {
            var convertedSteps = stepList.Select(sl => TryConvertStep(sl, expectedType))
                .Combine(ErrorBuilderList.Combine).Map(x=>x.ToList());

            if (convertedSteps.IsFailure)
                return convertedSteps.ConvertFailure<Unit>();

            stepListDictionary.Add(key, convertedSteps.Value);

            return Unit.Default;
        }

        private static Result<IFreezableStep, IErrorBuilder> TryConvertStep(IFreezableStep step, Type? expectedType)
        {
            if(expectedType == null)
                throw new ArgumentNullException(nameof(expectedType));

            if (step is ConstantFreezableStep cfs)
            {
                if (expectedType.IsGenericParameter || expectedType.IsInstanceOfType(cfs.Value))
                    return cfs;

                if(expectedType == typeof(Stream) && cfs.Value is string s)
                {
                    var bytes = Encoding.Default.GetBytes(s);
                    Stream stream = new MemoryStream(bytes);

                    return new ConstantFreezableStep(stream);
                }

                return new ErrorBuilder($"Could not convert '{cfs.Value}' from {cfs.Value.GetType().Name} to {expectedType.Name}", ErrorCode.InvalidCast);
            }

            return Result.Success<IFreezableStep, IErrorBuilder>(step);
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
        public Result<VariableName, IErrorBuilder> GetVariableName(string name, string typeName) => VariableNameDictionary.TryFindOrFail(name,
            ()=> ErrorHelper.MissingParameterError(name, typeName));

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep, IErrorBuilder> GetArgument(string name, string typeName) => StepDictionary.TryFindOrFail(name,
            ()=> ErrorHelper.MissingParameterError(name, typeName));

        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IErrorBuilder> GetListArgument(string name, string typeName) => StepListDictionary.TryFindOrFail(name,
            ()=> ErrorHelper.MissingParameterError(name, typeName));

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
            if (!DictionariesEqual1(VariableNameDictionary, other.VariableNameDictionary)) return false;
            if (!DictionariesEqual2(StepDictionary, other.StepDictionary)) return false;
            if (!DictionariesEqual3(StepListDictionary, other.StepListDictionary)) return false;
            return true;

            static bool DictionariesEqual1(IReadOnlyDictionary<string, VariableName> dict1, IReadOnlyDictionary<string, VariableName> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));

            static bool DictionariesEqual2(IReadOnlyDictionary<string, IFreezableStep> dict1, IReadOnlyDictionary<string, IFreezableStep> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));


            static bool DictionariesEqual3(IReadOnlyDictionary<string, IReadOnlyList<IFreezableStep>> dict1, IReadOnlyDictionary<string, IReadOnlyList<IFreezableStep>> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].SequenceEqual(dict2[key]));
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