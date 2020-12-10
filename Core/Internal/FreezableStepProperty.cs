using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Opt = OneOf.OneOf<Reductech.EDR.Core.Internal.VariableName, Reductech.EDR.Core.Internal.IFreezableStep, System.Collections.Immutable.ImmutableList<Reductech.EDR.Core.Internal.IFreezableStep>>;


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
        public FreezableStepProperty(VariableName variableName, IErrorLocation location)
        {
            Option = variableName;
            Location = location;
        }

        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(IFreezableStep step, IErrorLocation location)
        {
            Option = Opt.FromT1(step);
            Location = location;
        }

        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(ImmutableList<IFreezableStep> stepList, IErrorLocation location)
        {
            Option = stepList;
            Location = location;
        }

        /// <summary>
        /// The chosen option.
        /// </summary>
        private Opt Option { get; }

        /// <summary>
        /// The location where this stepMember comes from.
        /// </summary>
        public IErrorLocation Location { get; }

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
        public Maybe<ImmutableList<IFreezableStep>> StepList => Option.Match(
            x => Maybe<ImmutableList<IFreezableStep>>.None,
            x => Maybe<ImmutableList<IFreezableStep>>.None,
            Maybe<ImmutableList<IFreezableStep>>.From);



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
        /// Gets the stepMember if it is a list of freezable steps.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(string propertyName)
        {
            if (Option.TryPickT2(out var l, out _))
                return Result.Success<IReadOnlyList<IFreezableStep>, IError>(l);

            return Result.Failure<IReadOnlyList<IFreezableStep>, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.StepList).WithLocation(Location));
        }


        /// <summary>
        /// Tries to convert this step member to a FreezableStep
        /// </summary>
        public IFreezableStep ConvertToStep()
        {
            var r = Option.Match(
                MapVariableName,
                x => x,
                MapStepList);

            return r;


            IFreezableStep MapStepList(ImmutableList<IFreezableStep> stepList)
            {
                if (stepList.Any() && stepList.All(x => x is EntityConstantFreezable))
                { //Special case for entity stream
                    var entities = stepList
                        .Select(x => (EntityConstantFreezable) x)
                        .Select(x => x.Value).ToList();

                    var entityStream = EntityStream.Create(entities);

                    var c = new EntityStreamConstantFreezable(entityStream);
                    return c;
                }

                return MapStepListToArray(stepList);
            }

            IFreezableStep MapStepListToArray(ImmutableList<IFreezableStep> stepList)
            {
                var array = FreezableFactory.CreateFreezableList(stepList, null, Location);
                return array;
            }

            IFreezableStep MapVariableName(VariableName vn)
            {
                var getVariableStep = FreezableFactory.CreateFreezableGetVariable(vn, Location);
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