using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A factory for creating steps.
    /// </summary>
    public interface IStepFactory
    {
        /// <summary>
        /// Unique name for this type of step.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// The type of the step to create.
        /// </summary>
        public Type StepType { get; }

        /// <summary>
        /// The category of the step. Used for documentation.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Builds the name for a particular instance of a step.
        /// </summary>
        IStepNameBuilder StepNameBuilder { get; }

        /// <summary>
        /// Tries to get a reference to the output type of this step.
        /// </summary>
        Result<ITypeReference, IError> TryGetOutputTypeReference(FreezableStepData freezableStepData, TypeResolver typeResolver);

        /// <summary>
        /// If this variable is being set. Get the type reference it is being set to.
        /// </summary>
        Result<Maybe<ITypeReference>, IError> GetTypeReferencesSet(VariableName variableName,
            FreezableStepData freezableStepData, TypeResolver typeResolver) =>
            Maybe<ITypeReference>.None;

        /// <summary>
        /// Serializer to use for yaml serialization.
        /// </summary>
        IStepSerializer Serializer { get; }

        /// <summary>
        /// An object which can combine a step with the next step in the sequence.
        /// </summary>
        Maybe<IStepCombiner> StepCombiner { get; }

        /// <summary>
        /// Special requirements for this step.
        /// </summary>
        IEnumerable<Requirement> Requirements { get; }

        /// <summary>
        /// Try to create the instance of this type and set all arguments.
        /// </summary>
        Result<IStep, IError> TryFreeze(StepContext stepContext, FreezableStepData freezableStepData,
            Configuration? configuration);

        /// <summary>
        /// Human readable explanation of the output type.
        /// </summary>
        string OutputTypeExplanation { get; }

        /// <summary>
        /// Gets the type of this member.
        /// </summary>
        MemberType GetExpectedMemberType(string name);

        /// <summary>
        /// Gets all enum types used by this step.
        /// </summary>
        IEnumerable<Type> EnumTypes { get; }
    }
}