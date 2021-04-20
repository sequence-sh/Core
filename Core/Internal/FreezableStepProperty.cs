using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// Any member of a step.
/// </summary>
public abstract record FreezableStepProperty(TextLocation? Location)
{
    /// <summary>
    /// The member type of this Step Member.
    /// </summary>
    public abstract MemberType MemberType { get; }

    /// <summary>
    /// Gets the stepMember if it is a Variable.
    /// </summary>
    public virtual Result<VariableName, IError> AsVariableName(string parameterName)
    {
        return Result.Failure<VariableName, IError>(
            ErrorCode.WrongParameterType.ToErrorBuilder(
                    parameterName,
                    MemberType.VariableName,
                    MemberType
                )
                .WithLocation(Location ?? ErrorLocation.EmptyLocation)
        );
    }

    /// <summary>
    /// Gets the stepMember if it is a list of freezable steps.
    /// </summary>
    public virtual Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(string parameterName)
    {
        return Result.Failure<IReadOnlyList<IFreezableStep>, IError>(
            ErrorCode.WrongParameterType.ToErrorBuilder(parameterName, "Array/Sequence", MemberType)
                .WithLocation(Location ?? ErrorLocation.EmptyLocation)
        );
    }

    /// <summary>
    /// Tries to convert this step member to a FreezableStep
    /// </summary>
    public abstract IFreezableStep ConvertToStep();

    /// <summary>
    /// A variable name member
    /// </summary>
    public sealed record Variable
        (VariableName VName, TextLocation? Location) : FreezableStepProperty(Location)
    {
        /// <inheritdoc />
        public override Result<VariableName, IError> AsVariableName(string parameterName)
        {
            return VName;
        }

        /// <inheritdoc />
        public override IFreezableStep ConvertToStep()
        {
            return FreezableFactory.CreateFreezableGetVariable(VName, Location);
        }

        /// <inheritdoc />
        public override MemberType MemberType => MemberType.VariableName;
    }

    /// <summary>
    /// A step member
    /// </summary>
    public sealed record Step
        (IFreezableStep FreezableStep, TextLocation? Location) : FreezableStepProperty(Location)
    {
        /// <inheritdoc />
        public override MemberType MemberType => MemberType.Step;

        /// <inheritdoc />
        public override IFreezableStep ConvertToStep()
        {
            return FreezableStep;
        }
    }

    /// <summary>
    /// A step list member
    /// </summary>
    public sealed record StepList
        (ImmutableList<IFreezableStep> List, TextLocation? Location) : FreezableStepProperty(
            Location
        )
    {
        /// <inheritdoc />
        public override MemberType MemberType => MemberType.StepList;

        /// <inheritdoc />
        public override Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(
            string parameterName)
        {
            return List;
        }

        /// <inheritdoc />
        public override IFreezableStep ConvertToStep()
        {
            return FreezableFactory.CreateFreezableList(List, Location);
        }

        /// <summary>
        /// Equality comparison
        /// </summary>
        public bool Equals(StepList? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return List.SequenceEqual(other.List);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return List.Count;
        }
    }
}

}
