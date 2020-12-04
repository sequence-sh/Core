using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Core.Internal
{
    /// <summary>
    /// A step which can be frozen.
    /// </summary>
    public interface IFreezableStep : IEquatable<IFreezableStep>
    {
        /// <summary>
        /// The human-readable name of this step.
        /// </summary>
        string StepName { get; }

        /// <summary>
        /// Try to freeze this step.
        /// </summary>
        Result<IStep, IError> TryFreeze(StepContext stepContext);


        /// <summary>
        /// Gets the variables set by this step and its children and the types of those variables if they can be resolved at this time.
        /// Does not include reserved variables e.g. Entity
        /// Returns an error if the type name cannot be resolved
        /// </summary>
        Result<IReadOnlyCollection<(VariableName variableName, Maybe<ITypeReference>)>, IError> GetVariablesSet(TypeResolver typeResolver);


        /// <summary>
        /// The output type of this step. Will be unit if the step does not have an output.
        /// </summary>
        Result<ITypeReference, IError> TryGetOutputTypeReference(TypeResolver typeResolver);

        /// <summary>
        /// Tries to freeze this step.
        /// </summary>
        public Result<IStep, IError> TryFreeze(StepFactoryStore stepFactoryStore) =>
            StepContext.TryCreate(stepFactoryStore,this)
                .Bind(TryFreeze);
    }
}