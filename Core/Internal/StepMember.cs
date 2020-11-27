using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Any member of a step.
    /// </summary>
    public sealed class StepMember : IEquatable<StepMember>
    {
        /// <summary>
        /// Create a new Variable StepMember
        /// </summary>
        public StepMember(VariableName variableName) => Option = OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT0(variableName);

        /// <summary>
        /// Create a new IFreezableStep StepMember
        /// </summary>
        public StepMember(IFreezableStep argument) => Option = OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT1(argument);

        /// <summary>
        /// Create a new ListArgument StepMember
        /// </summary>
        public StepMember(IReadOnlyList<IFreezableStep> listArgument) => Option = OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>>.FromT2(listArgument);


        /// <summary>
        /// The member type of this Step Member.
        /// </summary>
        public MemberType MemberType
        {
            get
            {
                if (Option.IsT0) return MemberType.VariableName;
                if (Option.IsT1) return MemberType.Step;
                if (Option.IsT2) return MemberType.StepList;

                return MemberType.NotAMember;
            }
        }

        /// <summary>
        /// This, if it is a variableName
        /// </summary>
        public Maybe<VariableName> VariableName => Option.Match(x => x, x => Maybe<VariableName>.None, x => Maybe<VariableName>.None);

        /// <summary>
        /// This, if it is a FreezableStep
        /// </summary>
        public Maybe<IFreezableStep> FreezableStep => Option.Match(x => Maybe<IFreezableStep>.None,Maybe<IFreezableStep>.From,  x => Maybe<IFreezableStep>.None);

        /// <summary>
        /// This, if it is a StepList
        /// </summary>
        public Maybe<IReadOnlyList<IFreezableStep>> StepList => Option.Match(x => Maybe<IReadOnlyList<IFreezableStep>>.None,x => Maybe<IReadOnlyList<IFreezableStep>>.None,Maybe<IReadOnlyList<IFreezableStep>>.From);

        /// <summary>
        /// The chosen option.
        /// </summary>
        private OneOf<VariableName, IFreezableStep, IReadOnlyList<IFreezableStep>> Option { get; }

        /// <summary>
        /// Use this StepMember.
        /// </summary>
        public T Match<T>(Func<VariableName, T> handleVariableName, Func<IFreezableStep, T> handleArgument,
            Func<IReadOnlyList<IFreezableStep>, T> handleListArgument) =>
            Option.Match(handleVariableName, handleArgument, handleListArgument);


        /// <summary>
        /// Use this StepMember.
        /// </summary>
        public void Switch(Action<VariableName> handleVariableName, Action<IFreezableStep> handleArgument,
            Action<IReadOnlyList<IFreezableStep>> handleListArgument) =>
            Option.Switch(handleVariableName, handleArgument, handleListArgument);

        /// <summary>
        /// Gets the stepMember if it is a Variable.
        /// </summary>
        public Result<VariableName> AsVariableName(string propertyName) =>
            Option.TryPickT0(out var vn, out _)?
                vn :
                Result.Failure<VariableName>($"{propertyName} was a {MemberType}, not a Variable");

        /// <summary>
        /// Gets the stepMember if it is an argument.
        /// </summary>
        public Result<IFreezableStep> AsArgument(string propertyName) =>
            Option.TryPickT1(out var fs, out _)?
                Result.Success(fs) :
                Result.Failure<IFreezableStep>($"{propertyName} was a {MemberType}, not an argument");

        /// <summary>
        /// Gets the stepMember if it is a listArgument.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>> AsListArgument(string propertyName) =>
            Option.TryPickT2(out var l, out _)?
                Result.Success(l) :
                Result.Failure<IReadOnlyList<IFreezableStep>>($"{propertyName} was a {MemberType}, not an list argument");

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
            var r = Option.Match(
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
                return Match(x =>
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


            return Option.Match(
                vn => other.Option.TryPickT0(out var vn2, out _) && vn.Equals(vn2),
                fs => other.Option.TryPickT1(out var fs2, out _) && fs.Equals(fs2),
                fsl => other.Option.TryPickT2(out var fsl2, out _) && ListsAreEqual(fsl, fsl2)

            );

            static bool ListsAreEqual(IEnumerable<IFreezableStep> a1, IEnumerable<IFreezableStep> a2)
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
        public override int GetHashCode() => Option.Match(x => x.GetHashCode(), x => x.GetHashCode(), x => x.Count);

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