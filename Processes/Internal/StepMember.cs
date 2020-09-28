using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Processes.Internal
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
        public Option<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>> Option { get; }

        /// <summary>
        /// Use this StepMember.
        /// </summary>
        public T Join<T>(Func<VariableName, T> handleVariableName, Func<IFreezableStep, T> handleArgument,
            Func<IReadOnlyList<IFreezableStep>, T> handleListArgument) =>
            Option.Join(handleVariableName, handleArgument, handleListArgument);

        /// <summary>
        /// Gets the processMember if it is a VariableName.
        /// </summary>
        public Result<VariableName> AsVariableName(string propertyName) => Option.Choice1.ToResult($"{propertyName} was a {MemberType}, not a VariableName");

        /// <summary>
        /// Gets the processMember if it is an argument.
        /// </summary>
        public Result<IFreezableStep> AsArgument(string propertyName) => Option.Choice2.ToResult($"{propertyName} was a {MemberType}, not an argument");

        /// <summary>
        /// Gets the processMember if it is a listArgument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>> AsListArgument(string propertyName) => Option.Choice3.ToResult($"{propertyName} was a {MemberType}, not an list argument");

        /// <summary>
        /// Tries to convert this step member to a particular type.
        /// </summary>
        public Result<StepMember> TryConvert(MemberType convertType)
        {
            if (MemberType == convertType) return this;

            if (MemberType == MemberType.StepList && convertType == MemberType.Step)
            {
                var sequenceProcess = SequenceStepFactory.CreateFreezable(Option.Choice3.Value, null);
                return new StepMember(sequenceProcess);
            }

            if (MemberType == MemberType.VariableName && convertType == MemberType.Step)
            {
                var getVariableProcess = GetVariableStepFactory.CreateFreezable(Option.Choice1.Value);
                return new StepMember(getVariableProcess);
            }

            return Result.Failure<StepMember>("Could not convert");
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