namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A step that can be run.
/// </summary>
public interface IStep : ISerializable
{
    /// <summary>
    /// The name of this step.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Run this step and return the result, assuming it is the specified type.
    /// Does not activate the logging
    /// </summary>
    ValueTask<Result<T, IError>> Run<T>(IStateMonad stateMonad, CancellationToken cancellationToken)
        where T : ISCLObject;

    /// <summary>
    /// Run this step and return the result, assuming it is the specified type.
    /// Logs data about the step.
    /// </summary>
    ValueTask<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken);

    /// <summary>
    /// Verify that this step can be run with the current settings.
    /// </summary>
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore);

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
    /// Requirements for this step that can only be determined at runtime.
    /// </summary>
    IEnumerable<Requirement> RuntimeRequirements { get; }

    /// <summary>
    /// Whether this step can be evaluated to a constant value
    /// </summary>
    /// <returns></returns>
    bool HasConstantValue(IEnumerable<VariableName> providedVariables);

    /// <summary>
    /// Get the value of this step if it is a constant
    /// </summary>
    ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs);

    /// <summary>
    /// Get all parameters of this step and all nested steps
    /// </summary>
    /// <returns></returns>
    IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues();

    /// <summary>
    /// Try to coerce this step to a step of another type
    /// </summary>
    public virtual Result<IStep, IErrorBuilder> TryCoerce(string propertyName, Type desiredStepType)
    {
        if (desiredStepType.IsInstanceOfType(this))
            return Result.Success<IStep, IErrorBuilder>(this); //No coercion required

        if (desiredStepType.IsGenericType)
        {
            var nestedType = desiredStepType.GenericTypeArguments.First();

            if (nestedType.GetInterfaces().Contains(typeof(ISCLOneOf)))
            {
                var oneOfTypes = nestedType.GenericTypeArguments;

                foreach (var oneOfType in oneOfTypes)
                {
                    var stepType     = typeof(IStep<>).MakeGenericType(oneOfType);
                    var coerceResult = TryCoerce(propertyName, stepType);

                    if (coerceResult.IsSuccess)
                    {
                        var resultStep = OneOfStep.Create(nestedType, this);
                        return Result.Success<IStep, IErrorBuilder>(resultStep);
                    }
                }
            }
            else if (this is IConstantStep constantStep)
            {
                var conversionResult = constantStep.TryConvert(nestedType, propertyName);
                return conversionResult;
            }
            else if (nestedType == typeof(ISCLObject))
                return Result.Success<IStep, IErrorBuilder>(this);
        }

        return ErrorCode.InvalidCast.ToErrorBuilder(propertyName, Name);
    }
}

/// <summary>
/// A step that can be run.
/// </summary>
public interface IStep<T> : IStep, IRunnableStep<T> where T : ISCLObject { }

/// <summary>
/// Something that can be run like a step.
/// Note that the return type does not have to be ISCLObject
/// </summary>
public interface IRunnableStep<T>
{
    /// <summary>
    /// Run this step and return the result.
    /// </summary>
    ValueTask<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);
}
