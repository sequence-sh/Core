using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step that can be run.
/// </summary>
public interface IStep
{
    /// <summary>
    /// The name of this step.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Convert this Step into a FreezableStep.
    /// Useful for equality comparison.
    /// </summary>
    IFreezableStep Unfreeze();

    /// <summary>
    /// Run this step and return the result, assuming it is the specified type.
    /// </summary>
    Task<Result<T, IError>> Run<T>(IStateMonad stateMonad, CancellationToken cancellationToken);

    /// <summary>
    /// Verify that this step can be run with the current settings.
    /// </summary>
    public Result<Unit, IError> Verify(SCLSettings settings);

    /// <summary>
    /// Whether this term should be bracketed when serialized
    /// </summary>
    bool ShouldBracketWhenSerialized { get; }

    /// <summary>
    /// The text location for this step.
    /// </summary>
    public TextLocation? TextLocation { get; set; }

    /// <summary>
    /// The output type. Will be the generic type in IStep&lt;T&gt;
    /// </summary>
    Type OutputType { get; }

    /// <summary>
    /// Serialize this step.
    /// </summary>
    string Serialize();

    /// <summary>
    /// Requirements for this step that can only be determined at runtime.
    /// </summary>
    IEnumerable<Requirement> RuntimeRequirements { get; }

    /// <summary>
    /// If this step has a constant, state-independent value, calculate it.
    /// </summary>
    Maybe<EntityValue> TryConvertToEntityValue();
}

/// <summary>
/// A step that can be run.
/// </summary>
public interface IStep<T> : IStep
{
    /// <summary>
    /// Run this step and return the result.
    /// </summary>
    Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);
}

}
