using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Steps;
using Opt = OneOf.OneOf<Reductech.EDR.Core.Internal.VariableName, Reductech.EDR.Core.Internal.IFreezableStep, System.Collections.Generic.IReadOnlyList<Reductech.EDR.Core.Internal.IFreezableStep>>;



namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Any member of a step.
    /// </summary>
    public sealed class FreezableStepProperty : IEquatable<FreezableStepProperty>
    {
        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(Opt option, IErrorLocation location)
        {
            Option = option;
            Location = location;
        }

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
        /// The location where this stepMember comes from.
        /// </summary>
        public IErrorLocation Location { get; }

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
        private Opt Option { get; }

        /// <summary>
        /// Use this FreezableStepProperty.
        /// </summary>
        public T Match<T>(Func<VariableName, T> handleVariableName, Func<IFreezableStep, T> handleArgument,
            Func<IReadOnlyList<IFreezableStep>, T> handleListArgument) =>
            Option.Match(handleVariableName, handleArgument, handleListArgument);


        /// <summary>
        /// Use this FreezableStepProperty.
        /// </summary>
        public void Switch(Action<VariableName> handleVariableName, Action<IFreezableStep> handleArgument,
            Action<IReadOnlyList<IFreezableStep>> handleListArgument) =>
            Option.Switch(handleVariableName, handleArgument, handleListArgument);

        /// <summary>
        /// Gets the stepMember if it is a Variable.
        /// </summary>
        public Result<VariableName, IError> AsVariableName(string propertyName)
        {
            if (Option.TryPickT0(out var vn, out _))
                return Result.Success<VariableName, IError>(vn);

            return Result.Failure<VariableName, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.VariableName).WithLocation(Location));
        }

        /// <summary>
        /// Gets the stepMember if it is a freezable step.
        /// </summary>
        public Result<IFreezableStep, IError> AsStep(string propertyName)
        {
            if (Option.TryPickT1(out var s, out _))
                return Result.Success<IFreezableStep, IError>(s);

            return Result.Failure<IFreezableStep, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.Step).WithLocation(Location));
        }

        /// <summary>
        /// Gets the stepMember if it is a list of freezable steps.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(string propertyName)
        {
            if (Option.TryPickT2(out var l, out _))
                return Result.Success<IReadOnlyList<IFreezableStep>, IError>(l);

            return Result.Failure<IReadOnlyList<IFreezableStep>, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.StepList).WithLocation(Location));
        }

        ///// <summary>
        ///// Tries to convert a step member of one type to one of another.
        ///// </summary>
        //public Result<FreezableStepProperty, IErrorBuilder> TryConvert(MemberType newMemberType, bool convertStepListToSequence)
        //{
        //    if (newMemberType == MemberType)
        //        return this;
        //    else if(newMemberType == MemberType.Step)
        //        return new FreezableStepProperty(Opt.FromT1(ConvertToStep(convertStepListToSequence)), Location);

        //    return new ErrorBuilder($"Could not convert {MemberType} to {newMemberType}", ErrorCode.InvalidCast);

        //}

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


            IFreezableStep MapStepList(IReadOnlyList<IFreezableStep> stepList, bool convertStepListToSequence1)
            {
                if (stepList.Any() && stepList.All(x => x is ConstantFreezableStep cfs && cfs.Value.IsT6))
                {
                    var entities = stepList
                        .Select(x => (ConstantFreezableStep) x)
                        .Select(x => x.Value.AsT6).ToList();

                    var entityStream = EntityStream.Create(entities);

                    var c = new ConstantFreezableStep(entityStream);
                    return c;
                }
                else if (convertStepListToSequence1)
                    return MapStepListToSequence(stepList);
                else return MapStepListToArray(stepList);
            }

            IFreezableStep MapStepListToArray(IReadOnlyList<IFreezableStep> stepList)
            {
                var array = ArrayStepFactory.CreateFreezable(stepList, null, Location);
                return array;
            }

            IFreezableStep MapStepListToSequence(IReadOnlyList<IFreezableStep> stepList)
            {
                var array = SequenceStepFactory.CreateFreezable(stepList, null, Location);
                return array;
            }

            IFreezableStep MapVariableName(VariableName vn)
            {
                var getVariableStep = GetVariableStepFactory.CreateFreezable(vn, Location);
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
        public bool Equals(FreezableStepProperty? other)
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
                //if (a1 is null)
                //    return a2 is null;

                //if (a2 is null)
                //    return false;

                return a1.SequenceEqual(a2);
            }
        }



        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepProperty other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Option.Match(x => x.GetHashCode(), x => x.GetHashCode(), x => x.Count);

        /// <summary>
        /// Equals operator
        /// </summary>
        public static bool operator ==(FreezableStepProperty? left, FreezableStepProperty? right) => Equals(left, right);

        /// <summary>
        /// Not Equals operator
        /// </summary>
        public static bool operator !=(FreezableStepProperty? left, FreezableStepProperty? right) => !Equals(left, right);
    }
}