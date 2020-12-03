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
        ///// <summary>
        ///// Try to create a new FreezableStepData
        ///// </summary>
        //public static Result<FreezableStepData, IErrorBuilder> TryCreate(IStepFactory stepFactory, IReadOnlyDictionary<string, FreezableStepProperty> dictionary)
        //{
        //    var stepDictionary = new Dictionary<string, IFreezableStep>();
        //    var variableNameDictionary = new Dictionary<string, VariableName>();
        //    var stepListDictionary = new Dictionary<string, IReadOnlyList<IFreezableStep>>();

        //    var remainingRequiredProperties = stepFactory.RequiredProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);

        //    var errors = new List<IErrorBuilder>();

        //    foreach (var (key, value) in dictionary)
        //    {
        //        var (memberType, type) = stepFactory.GetExpectedMemberType(key);

        //        if (memberType == MemberType.NotAMember)
        //            errors.Add(ErrorHelper.UnexpectedParameterError(key, stepFactory.TypeName));
        //        else
        //        {
        //            var convertedMember = value.TryConvert(memberType, false);

        //            if(convertedMember.IsFailure)
        //                errors.Add(convertedMember.Error);
        //            else
        //            {
        //                remainingRequiredProperties.Remove(key);

        //                var addResult =
        //                convertedMember.Value
        //                    .Match(
        //                    x => AddVariableName(variableNameDictionary, key, x),
        //                    x => TryAddStep(stepDictionary, key, x, type),
        //                    x => TryAddStepList(stepListDictionary, key, x, type)
        //                    );
        //                if(addResult.IsFailure)
        //                    errors.Add(addResult.Error);
        //            }
        //        }
        //    }

        //    foreach (var remainingRequiredProperty in remainingRequiredProperties)
        //        errors.Add(ErrorHelper.MissingParameterError(remainingRequiredProperty, stepFactory.TypeName));

        //    if (errors.Any())
        //        return Result.Failure<FreezableStepData, IErrorBuilder>( ErrorBuilderList.Combine(errors));

        //    return new FreezableStepData(stepDictionary, variableNameDictionary, stepListDictionary);
        //}

        //private static Result<Unit, IErrorBuilder> AddVariableName(IDictionary<string, VariableName> variableNames, string key, VariableName variableName)
        //{
        //    variableNames.Add(key, variableName);
        //    return Unit.Default;
        //}

        //private static Result<Unit, IErrorBuilder> TryAddStep(IDictionary<string, IFreezableStep> freezableSteps, string key, IFreezableStep freezableStep, Type? expectedType)
        //{
        //    var convertedResult = TryConvertStep(freezableStep, expectedType);
        //    if (convertedResult.IsFailure)
        //        return convertedResult.ConvertFailure<Unit>();

        //    freezableSteps.Add(key, convertedResult.Value);

        //    return Unit.Default;
        //}

        //private static Result<Unit, IErrorBuilder> TryAddStepList(IDictionary<string, IReadOnlyList<IFreezableStep>> stepListDictionary, string key, IReadOnlyList<IFreezableStep> stepList, Type? expectedType)
        //{
        //    var convertedSteps = stepList.Select(sl => TryConvertStep(sl, expectedType))
        //        .Combine(ErrorBuilderList.Combine).Map(x=>x.ToList());

        //    if (convertedSteps.IsFailure)
        //        return convertedSteps.ConvertFailure<Unit>();

        //    stepListDictionary.Add(key, convertedSteps.Value);

        //    return Unit.Default;
        //}

        //private static Result<IFreezableStep, IErrorBuilder> TryConvertStep(IFreezableStep step, Type? expectedType)
        //{
        //    if(expectedType == null)
        //        throw new ArgumentNullException(nameof(expectedType));

        //    if (step is ConstantFreezableStep cfs)
        //    {
        //        if (expectedType.IsGenericParameter || expectedType.IsInstanceOfType(cfs.Value))
        //            return cfs;

        //        if(expectedType == typeof(Stream) && cfs.Value is string s)
        //        {
        //            var bytes = Encoding.Default.GetBytes(s);
        //            Stream stream = new MemoryStream(bytes);

        //            return new ConstantFreezableStep(stream);
        //        }

        //        return new ErrorBuilder($"Could not convert '{cfs.Value}' from {cfs.Value.GetType().Name} to {expectedType.Name}", ErrorCode.InvalidCast);
        //    }

        //    return Result.Success<IFreezableStep, IErrorBuilder>(step);
        //}

        /// <summary>
        /// Creates a new FreezableStepData
        /// </summary>
        public FreezableStepData(IReadOnlyDictionary<string, FreezableStepProperty> steps, IErrorLocation location)
        {
            Steps = steps;
            Location = location;
        }

        public IReadOnlyDictionary<string, FreezableStepProperty> Steps { get; }

        public IErrorLocation Location {get;}

        /// <summary>
        /// Gets a variable name.
        /// </summary>
        public Result<VariableName, IError> GetVariableName(string name, string typeName) =>
            Steps.TryFindOrFail(name,
            ()=> ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x=>x.AsVariableName(name)
                    );

        /// <summary>
        /// Gets an argument.
        /// </summary>
        public Result<IFreezableStep, IError> GetStep(string name, string typeName) =>
            Steps.TryFindOrFail(name,
            () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Map(x => x.ConvertToStep());


        /// <summary>
        /// Gets a list argument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> GetStepList(string name, string typeName) =>

            Steps.TryFindOrFail(name,
            () => ErrorHelper.MissingParameterError(name, typeName).WithLocation(Location))
                .Bind(x => x.AsStepList(name)
                    );

        /// <inheritdoc />
        public override string ToString()
        {
            var keys = Steps.OrderBy(x=>x);
            var keyString = string.Join("; ", keys);

            return keyString;
        }

        /// <inheritdoc />
        public bool Equals(FreezableStepData? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return DictionariesEqual1(Steps, other.Steps);

            static bool DictionariesEqual1(IReadOnlyDictionary<string, FreezableStepProperty> dict1, IReadOnlyDictionary<string, FreezableStepProperty> dict2) =>
                dict1.Count == dict2.Count &&
                dict1.Keys.All(key => dict2.ContainsKey(key) && dict1[key].Equals(dict2[key]));
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepData other && Equals(this, other);

        /// <inheritdoc />
        public override int GetHashCode() => Steps.Count;

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