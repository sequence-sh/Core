using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;
using Entity = CSharpFunctionalExtensions.Entity;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Any member of a step.
    /// </summary>
    public sealed class StepMember : IEquatable<StepMember>
    {
        /// <summary>
        /// Create a new VariableName StepMember
        /// </summary>
        public StepMember(VariableName variableName) => Option = new Option<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>(variableName);

        /// <summary>
        /// Create a new IFreezableStep StepMember
        /// </summary>
        public StepMember(IFreezableStep argument) => Option = new Option<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>(argument);

        /// <summary>
        /// Create a new ListArgument StepMember
        /// </summary>
        public StepMember(IReadOnlyList<IFreezableStep> listArgument) => Option = new Option<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>(listArgument);


        /// <summary>
        /// The member type of this Step Member.
        /// </summary>
        public MemberType MemberType
        {
            get
            {
                if (Option.Choice1.HasValue) return MemberType.VariableName;
                if (Option.Choice2.HasValue) return MemberType.Step;
                if (Option.Choice3.HasValue) return MemberType.StepList;

                return MemberType.NotAMember;
            }
        }

        /// <summary>
        /// The chosen option.
        /// </summary>
        private Option<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>> Option { get; }

        /// <summary>
        /// Use this StepMember.
        /// </summary>
        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableStep, T> handleArgument,
            Func<IReadOnlyList<IFreezableStep>, T> handleListArgument) =>
            Option.Join(handleVariableName, handleArgument, handleListArgument);


        /// <summary>
        /// Use this StepMember.
        /// </summary>
        public void Match(Action<VariableName> handleVariableName, Action<IFreezableStep> handleArgument,
            Action<IReadOnlyList<IFreezableStep>> handleListArgument) =>
            Option.Match(handleVariableName, handleArgument, handleListArgument);

        /// <summary>
        /// Gets the stepMember if it is a VariableName.
        /// </summary>
        public Result<VariableName> AsVariableName(string propertyName) => Option.Choice1.ToResult($"{propertyName} was a {MemberType}, not a VariableName");

        /// <summary>
        /// Gets the stepMember if it is an argument.
        /// </summary>
        public Result<IFreezableStep> AsArgument(string propertyName) => Option.Choice2.ToResult($"{propertyName} was a {MemberType}, not an argument");

        /// <summary>
        /// Gets the stepMember if it is a listArgument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>> AsListArgument(string propertyName) => Option.Choice3.ToResult($"{propertyName} was a {MemberType}, not an list argument");

        /// <summary>
        /// Tries to convert a step member of one type to one of another.
        /// </summary>
        public Result<StepMember, IErrorBuilder> TryConvert(MemberType newMemberType, bool convertStepListToSequence)
        {
            if (newMemberType == MemberType)
                return this;
            else if(newMemberType == MemberType.Step)
                return new StepMember(ConvertToStep(convertStepListToSequence));

            return new ErrorBuilder($"Could not convert {MemberType} to {newMemberType}", ErrorCode.InvalidCast);

        }

        /// <summary>
        /// Tries to convert this step member to a FreezableStep
        /// </summary>
        public IFreezableStep ConvertToStep(bool convertStepListToSequence)
        {

            var r = Option.Join(
                MapVariableName,
                x => x,

                x => MapStepList(x, convertStepListToSequence));

            return r;


            static IFreezableStep MapStepList(IReadOnlyList<IFreezableStep> stepList, bool convertStepListToSequence)
            {
                if (stepList.Any() && stepList.All(x => x is ConstantFreezableStep cfs && cfs.Value is Entities.Entity))
                {
                    var entities = stepList
                        .Select(x => (ConstantFreezableStep) x)
                        .Select(x => (Entities.Entity) x.Value).ToList();

                    var entityStream = EntityStream.Create(entities);

                    var c = new ConstantFreezableStep(entityStream);
                    return c;
                }
                else if (convertStepListToSequence)
                    return MapStepListToSequence(stepList);
                else return MapStepListToArray(stepList);
            }

            static IFreezableStep MapStepListToArray(IReadOnlyList<IFreezableStep> stepList)
            {
                var array = ArrayStepFactory.CreateFreezable(stepList, null);
                return array;
            }

            static IFreezableStep MapStepListToSequence(IReadOnlyList<IFreezableStep> stepList)
            {
                var array = SequenceStepFactory.CreateFreezable(stepList, null);
                return array;
            }

            static IFreezableStep MapVariableName(VariableName vn)
            {
                var getVariableStep = GetVariableStepFactory.CreateFreezable(vn);
                return getVariableStep;
            }
        }

        /// <summary>
        /// A string representation of the member.
        /// </summary>
        public string MemberString
        {
            get
            {
                return Join(x =>
                        x.ToString()!,
                    x => x.ToString()!,
                    x => x.ToString()!);

            }
        }

        /// <inheritdoc />
        public override string ToString() => new {MemberType, Value=MemberString}.ToString()!;

        /// <inheritdoc />
        public bool Equals(StepMember? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Option.Equals(other.Option,
                (a, b) => a.Equals(b),
                (a, b) => a.Equals(b),
                ListsAreEqual);

            static bool ListsAreEqual(IReadOnlyList<IFreezableStep> a1, IReadOnlyList<IFreezableStep> a2)
            {
                if (a1 is null)
                    return a2 is null;

                if (a2 is null)
                    return false;

                return a1.SequenceEqual(a2);
            }
        }



        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is StepMember other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(Option.Choice1, Option.Choice2, Option.Choice3.Select(x=>x.Count));

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(StepMember? left, StepMember? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(StepMember? left, StepMember? right) => !Equals(left, right);
    }
}