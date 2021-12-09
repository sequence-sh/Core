using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal;

/// <summary>
/// Information about how this step was passed
/// </summary>
public sealed record StepMetadata(bool PassedAsInfix, bool Bracketed)
{
    /// <summary>
    /// Default Step Metadata
    /// </summary>
    public static readonly StepMetadata Default = new(false, false);
}

/// <summary>
/// Any member of a step.
/// </summary>
public abstract record FreezableStepProperty(TextLocation Location)
{
    /// <summary>
    /// The member type of this Step Member.
    /// </summary>
    public abstract MemberType MemberType { get; }

    /// <summary>
    /// Information about how this step was passed
    /// </summary>
    public StepMetadata StepMetadata { get; init; } = StepMetadata.Default;

    /// <summary>
    /// Gets the stepMember if it is a Variable.
    /// </summary>
    public virtual Result<VariableName, IError> AsVariableName(string parameterName)
    {
        return Result.Failure<VariableName, IError>(
            ErrorCode.WrongType.ToErrorBuilder(
                    "Step",
                    nameof(VariableName),
                    parameterName,
                    "Value",
                    MemberType.ToString()
                )
                .WithLocation(Location)
        );
    }

    /// <summary>
    /// Gets the stepMember if it is a list of freezable steps.
    /// </summary>
    public virtual Result<IReadOnlyList<IFreezableStep>, IError> AsStepList(string parameterName)
    {
        return Result.Failure<IReadOnlyList<IFreezableStep>, IError>(
            ErrorCode.WrongType.ToErrorBuilder(
                    "Step",
                    "Array/Sequence",
                    parameterName,
                    "Value",
                    MemberType.ToString()
                )
                .WithLocation(Location)
        );
    }

    /// <summary>
    /// Tries to convert this step member to a FreezableStep
    /// </summary>
    public abstract IFreezableStep ConvertToStep();

    /// <summary>
    /// Tries to convert this step member to a Lambda
    /// </summary>
    /// <returns></returns>
    public virtual Lambda ConvertToLambda()
    {
        return new Lambda(null, ConvertToStep(), Location);
    }

    /// <summary>
    /// Move named arguments up a level if necessary
    /// </summary>
    public abstract FreezableStepProperty ReorganizeNamedArguments(
        StepFactoryStore stepFactoryStore);

    /// <summary>
    /// A variable name member
    /// </summary>
    public sealed record Variable
        (VariableName VName, TextLocation Location) : FreezableStepProperty(Location)
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
        public override FreezableStepProperty ReorganizeNamedArguments(
            StepFactoryStore stepFactoryStore)
        {
            return this;
        }

        /// <inheritdoc />
        public override MemberType MemberType => MemberType.VariableName;
    }

    /// <summary>
    /// A Lambda Function
    /// </summary>
    public sealed record Lambda(
        VariableName? VName,
        IFreezableStep FreezableStep,
        TextLocation Location) : FreezableStepProperty(Location)
    {
        /// <inheritdoc />
        public override MemberType MemberType => MemberType.Lambda;

        /// <summary>
        /// The VariableName or Item
        /// </summary>
        public VariableName VariableNameOrItem => VName ?? VariableName.Item;

        /// <inheritdoc />
        public override IFreezableStep ConvertToStep()
        {
            return FreezableStep; //This may not be correct //TODO remove
        }

        /// <inheritdoc />
        public override Lambda ConvertToLambda()
        {
            return this;
        }

        /// <inheritdoc />
        public override FreezableStepProperty ReorganizeNamedArguments(
            StepFactoryStore stepFactoryStore)
        {
            var r = FreezableStep.ReorganizeNamedArguments(stepFactoryStore);
            return this with { FreezableStep = r };
        }
    }

    /// <summary>
    /// A step member
    /// </summary>
    public sealed record Step
        (IFreezableStep FreezableStep, TextLocation Location) : FreezableStepProperty(Location)
    {
        /// <inheritdoc />
        public override MemberType MemberType => MemberType.Step;

        /// <inheritdoc />
        public override IFreezableStep ConvertToStep()
        {
            return FreezableStep;
        }

        /// <inheritdoc />
        public override FreezableStepProperty ReorganizeNamedArguments(
            StepFactoryStore stepFactoryStore)
        {
            var r = FreezableStep.ReorganizeNamedArguments(stepFactoryStore);

            return this with { FreezableStep = r };
        }
    }

    /// <summary>
    /// A step list member
    /// </summary>
    public sealed record StepList
        (ImmutableList<IFreezableStep> List, TextLocation Location) : FreezableStepProperty(
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

        /// <inheritdoc />
        public override FreezableStepProperty ReorganizeNamedArguments(
            StepFactoryStore stepFactoryStore)
        {
            var r = List.Select(x => x.ReorganizeNamedArguments(stepFactoryStore))
                .ToImmutableList();

            return this with { List = r };
        }
    }
}
