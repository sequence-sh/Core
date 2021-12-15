namespace Reductech.EDR.Core.Internal;

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
    /// Run this step and return the result, assuming it is the specified type.
    /// </summary>
    Task<Result<T, IError>> Run<T>(IStateMonad stateMonad, CancellationToken cancellationToken)
        where T : ISCLObject;

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
    /// Serialize this step.
    /// </summary>
    string Serialize(SerializeOptions options);

    /// <summary>
    /// Requirements for this step that can only be determined at runtime.
    /// </summary>
    IEnumerable<Requirement> RuntimeRequirements { get; }

    /// <summary>
    /// Get the value of this step if it is constant
    /// </summary>
    Maybe<ISCLObject> TryGetConstantValue();

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
    Task<Result<T, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken);
}
