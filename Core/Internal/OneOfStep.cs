using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OneOf;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

/// <summary>
/// A step that could have more than one possible type
/// </summary>
public abstract class OneOfStep : IStep
{
    /// <summary>
    /// The actual value of the step
    /// </summary>
    protected abstract IStep StepValue { get; }

    /// <inheritdoc />
    public Task<Result<T, IError>> Run<T>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return StepValue.Run<T>(stateMonad, cancellationToken);
    }

    /// <inheritdoc />
    public string Name => StepValue.Name;

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore)
    {
        return StepValue.Verify(stepFactoryStore);
    }

    /// <inheritdoc />
    public bool ShouldBracketWhenSerialized => StepValue.ShouldBracketWhenSerialized;

    /// <inheritdoc />
    public TextLocation? TextLocation
    {
        get => StepValue.TextLocation;
        set => StepValue.TextLocation = value;
    }

    /// <inheritdoc />
    public abstract Type OutputType { get; }

    /// <inheritdoc />
    public string Serialize()
    {
        return StepValue.Serialize();
    }

    /// <inheritdoc />
    public IEnumerable<Requirement> RuntimeRequirements => StepValue.RuntimeRequirements;

    /// <inheritdoc />
    public Maybe<EntityValue> TryConvertToEntityValue()
    {
        return StepValue.TryConvertToEntityValue();
    }

    /// <summary>
    /// Create a OneOfStep
    /// </summary>
    public static OneOfStep Create(Type oneOfType, IStep actualStep)
    {
        var oneOfTypes = oneOfType.GenericTypeArguments;

        Type genericStepType;

        if (oneOfTypes.Length == 2)
            genericStepType = typeof(OneOfStep<,>);
        else if (oneOfTypes.Length == 3)
            genericStepType = typeof(OneOfStep<,,>);

        else
            throw new Exception($"Cannot create a OneOf with {oneOfTypes.Length} type arguments");

        var actualType = genericStepType.MakeGenericType(oneOfTypes);

        var resultStep = (OneOfStep)Activator.CreateInstance(
            actualType,
            actualStep
        )!;

        return resultStep;
    }
}

/// <summary>
/// A step that could have one of two possible types
/// </summary>
public class OneOfStep<T0, T1> : OneOfStep, IStep<OneOf<T0, T1>>
{
    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    private OneOfStep(OneOf<IStep<T0>, IStep<T1>> step)
    {
        Step = step;
    }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T0> step0) : this(OneOf<IStep<T0>, IStep<T1>>.FromT0(step0)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T1> step1) : this(OneOf<IStep<T0>, IStep<T1>>.FromT1(step1)) { }

    /// <summary>
    /// The step
    /// </summary>
    public OneOf<IStep<T0>, IStep<T1>> Step { get; }

    /// <inheritdoc />
    protected override IStep StepValue => Step.Match(x => x as IStep, x => x);

    /// <inheritdoc />
    public override Type OutputType => typeof(OneOf<T0, T1>);

    /// <inheritdoc />
    public Task<Result<OneOf<T0, T1>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Step.Match(
            x => x.Run(stateMonad, cancellationToken).Map(OneOf<T0, T1>.FromT0),
            x => x.Run(stateMonad, cancellationToken).Map(OneOf<T0, T1>.FromT1)
        );
    }
}

/// <summary>
/// A step that could have one of two possible types
/// </summary>
public class OneOfStep<T0, T1, T2> : OneOfStep, IStep<OneOf<T0, T1, T2>>
{
    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    private OneOfStep(OneOf<IStep<T0>, IStep<T1>, IStep<T2>> step)
    {
        Step = step;
    }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T0> step0) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT0(step0)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T1> step1) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT1(step1)) { }

    /// <summary>
    /// Create a new OneOfStep
    /// </summary>
    public OneOfStep(IStep<T2> step2) :
        this(OneOf<IStep<T0>, IStep<T1>, IStep<T2>>.FromT2(step2)) { }

    /// <summary>
    /// The step
    /// </summary>
    public OneOf<IStep<T0>, IStep<T1>, IStep<T2>> Step { get; }

    /// <inheritdoc />
    protected override IStep StepValue => Step.Match(x => x as IStep, x => x, x => x);

    /// <inheritdoc />
    public override Type OutputType => typeof(OneOf<T0, T1, T2>);

    /// <inheritdoc />
    public Task<Result<OneOf<T0, T1, T2>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return Step.Match(
            x => x.Run(stateMonad, cancellationToken).Map(OneOf<T0, T1, T2>.FromT0),
            x => x.Run(stateMonad, cancellationToken).Map(OneOf<T0, T1, T2>.FromT1),
            x => x.Run(stateMonad, cancellationToken).Map(OneOf<T0, T1, T2>.FromT2)
        );
    }
}

}
