using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Internal.Errors;


namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// Any member of a step.
    /// </summary>
    public sealed class FreezableStepProperty : OneOfBase<VariableName, IFreezableStep, ImmutableList<IFreezableStep>>, IEquatable<FreezableStepProperty>
    {
        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(VariableName variableName, IErrorLocation location)
            : base(variableName) => Location = location;

        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(IFreezableStep step, IErrorLocation location)
            : base(OneOf<VariableName, IFreezableStep, ImmutableList<IFreezableStep>>.FromT1(step)) => Location = location;

        /// <summary>
        /// Create a new FreezableStepProperty
        /// </summary>
        public FreezableStepProperty(ImmutableList<IFreezableStep> stepList, IErrorLocation location)
            : base(stepList) => Location = location;

        /// <summary>
        /// The location where this stepMember comes from.
        /// </summary>
        public IErrorLocation Location { get; }

        /// <summary>
        /// The member type of this Step Member.
        /// </summary>
        public MemberType MemberType
            => Match(_ => MemberType.VariableName, _ => MemberType.Step, _ => MemberType.StepList);


        /// <summary>
        /// This, if it is a variableName
        /// </summary>
        public Maybe<VariableName> VariableName => Match(x => x, _ => Maybe<VariableName>.None, _ => Maybe<VariableName>.None);

        /// <summary>
        /// Gets the stepMember if it is a Variable.
        /// </summary>
        public Result<VariableName, IError> AsVariableName(string propertyName)
        {
            if (TryPickT0(out var vn, out _))
                return Result.Success<VariableName, IError>(vn);

            return Result.Failure<VariableName, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.VariableName).WithLocation(Location));
        }


        /// <summary>
        /// Gets the stepMember if it is a list of freezable steps.
        /// </summary>
        public Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(string propertyName)
        {
            if (TryPickT2(out var l, out _))
                return Result.Success<IReadOnlyList<IFreezableStep>, IError>(l);

            return Result.Failure<IReadOnlyList<IFreezableStep>, IError>(
                ErrorHelper.WrongParameterTypeError(propertyName, MemberType, MemberType.StepList).WithLocation(Location));
        }


        /// <summary>
        /// Tries to convert this step member to a FreezableStep
        /// </summary>
        public IFreezableStep ConvertToStep()
        {
            var r = Match(
                MapVariableName,
                x => x,
                MapStepList);

            return r;


            IFreezableStep MapStepList(ImmutableList<IFreezableStep> stepList) => MapStepListToArray(stepList);

            IFreezableStep MapStepListToArray(ImmutableList<IFreezableStep> stepList) => FreezableFactory.CreateFreezableList(stepList, null, Location);

            IFreezableStep MapVariableName(VariableName vn) => FreezableFactory.CreateFreezableGetVariable(vn, Location);
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


            return Match(
                vn => other.TryPickT0(out var vn2, out _) && vn.Equals(vn2),
                fs => other.TryPickT1(out var fs2, out _) && fs.Equals(fs2),
                fsl => other.TryPickT2(out var fsl2, out _) && ListsAreEqual(fsl, fsl2)

            );

            static bool ListsAreEqual(IEnumerable<IFreezableStep> a1, IEnumerable<IFreezableStep> a2) => a1.SequenceEqual(a2);
        }



        /// <inheritdoc />
        public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FreezableStepProperty other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => Match(x => x.GetHashCode(), x => x.GetHashCode(), x => x.Count);

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