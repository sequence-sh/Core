using Microsoft.Extensions.Logging.Abstractions;
using Reductech.Sequence.Core.Abstractions;
using Reductech.Sequence.Core.Internal.Logging;

namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A runnable step that is not a constant.
/// </summary>
public abstract class CompoundStep<T> : ICompoundStep<T> where T : ISCLObject
{
    /// <summary>
    /// Run this step.
    /// Does not activate logging.
    /// </summary>
    protected abstract ValueTask<Result<T, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken);

    /// <inheritdoc />
    public async ValueTask<Result<ISCLObject, IError>> RunUntyped(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var r = await (this as IRunnableStep<T>).Run(stateMonad, cancellationToken)
            .Map(x => x as ISCLObject);

        return r;
    }

    /// <inheritdoc />
    async ValueTask<Result<T, IError>> IRunnableStep<T>.Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        using (stateMonad.Logger.BeginScope(Name))
        {
            object[] GetEnterStepArgs()
            {
                var properties = AllProperties
                    .ToDictionary(x => x.Name, x => x.Serialize(SerializeOptions.SanitizedName));

                return new object[] { Name, properties };
            }

            LogSituation.EnterStep.Log(
                stateMonad,
                this,
                GetEnterStepArgs()
            ); //TODO do not create dictionary unless required

            var result = await Run(stateMonad, cancellationToken);

            if (result.IsFailure)
            {
                LogSituation.ExitStepFailure.Log(stateMonad, this, Name, result.Error.AsString);
            }
            else
            {
                var resultValue = result.Value.Serialize(SerializeOptions.SanitizedName);

                LogSituation.ExitStepSuccess.Log(stateMonad, this, Name, resultValue);
            }

            return result;
        }
    }

    /// <inheritdoc />
    public virtual ValueTask<Result<T1, IError>> Run<T1>(
        IStateMonad stateMonad,
        CancellationToken cancellationToken) where T1 : ISCLObject
    {
        return Run(stateMonad, cancellationToken)
            .BindCast<T, T1, IError>(
                ErrorCode.InvalidCast.ToErrorBuilder(typeof(T), typeof(T1)).WithLocation(this)
            );
    }

    /// <summary>
    /// The factory used to create steps of this type.
    /// </summary>
    public abstract IStepFactory StepFactory { get; }

    /// <inheritdoc />
    public string Serialize(SerializeOptions options) =>
        StepFactory.Serializer.Serialize(options, AllProperties);

    /// <inheritdoc />
    public void Format(
        IndentationStringBuilder indentationStringBuilder,
        FormattingOptions options,
        Stack<Comment> remainingComments)
    {
        StepFactory.Serializer.Format(
            AllProperties,
            TextLocation,
            indentationStringBuilder,
            options,
            remainingComments
        );
    }

    /// <inheritdoc />
    public virtual string Name => StepFactory.TypeName;

    /// <inheritdoc />
    public override string ToString() => Name;

    /// <summary>
    /// The text location for this step.
    /// </summary>
    public TextLocation? TextLocation { get; set; }

    /// <inheritdoc />
    public virtual IEnumerable<Requirement> RuntimeRequirements
    {
        get
        {
            var connectorName = GetType().Assembly.GetName().Name!;

            var requirements = AllProperties
                .SelectMany(x => x.RequiredVersions)
                .Select(x => x.ToRequirement(connectorName))
                .ToList();

            var nestedRequirements = AllProperties.SelectMany(GetNestedRequirements).ToList();

            requirements.AddRange(nestedRequirements);

            var groupedRequirements = Requirement.CompressRequirements(requirements).ToList();
            return groupedRequirements;

            static IEnumerable<Requirement> GetNestedRequirements(StepProperty stepProperty)
            {
                return stepProperty switch
                {
                    StepProperty.SingleStepProperty ssp => ssp.Step.GetAllRequirements(),
                    StepProperty.StepListProperty slp => slp.StepList.SelectMany(
                        x => x.GetAllRequirements()
                    ),
                    _ => new List<Requirement>()
                };
            }
        }
    }

    /// <inheritdoc />
    public bool HasConstantValue(IEnumerable<VariableName> providedVariables)
    {
        if (!GetType().GetCustomAttributes<AllowConstantFoldingAttribute>().Any())
            return false;

        // ReSharper disable PossibleMultipleEnumeration
        foreach (var stepProperty in AllProperties)
        {
            switch (stepProperty)
            {
                case StepProperty.LambdaFunctionProperty lambdaFunctionProperty:
                {
                    var newVariables = providedVariables
                        .Append(lambdaFunctionProperty.LambdaFunction.VariableNameOrItem);

                    if (!lambdaFunctionProperty.LambdaFunction.Step.HasConstantValue(newVariables))
                        return false;

                    break;
                }
                case StepProperty.SingleStepProperty singleStepProperty:
                {
                    if (!singleStepProperty.Step.HasConstantValue(providedVariables))
                        return false;

                    break;
                }
                case StepProperty.StepListProperty stepListProperty:
                {
                    foreach (var step in stepListProperty.StepList)
                    {
                        if (!step.HasConstantValue(providedVariables))
                            return false;
                    }

                    break;
                }

                case StepProperty.VariableNameProperty variableNameProperty:
                {
                    if (!providedVariables.Contains(variableNameProperty.VariableName))
                        return false; //We do not have the value of this variable

                    break;
                }

                default: throw new ArgumentOutOfRangeException(nameof(stepProperty));
            }
        }
        // ReSharper restore PossibleMultipleEnumeration

        return true;
    }

    /// <inheritdoc />
    public async ValueTask<Maybe<ISCLObject>> TryGetConstantValueAsync(
        IReadOnlyDictionary<VariableName, ISCLObject> variableValues,
        StepFactoryStore sfs)
    {
        if (!HasConstantValue(variableValues.Keys))
            return Maybe<ISCLObject>.None;

        var stateMonad = new StateMonad(
            NullLogger.Instance,
            sfs,
            NullExternalContext.Instance,
            ImmutableDictionary<string, object>.Empty
        );

        var r1 = stateMonad.SetInitialVariablesAsync(variableValues).Result;

        if (r1.IsSuccess)
        {
            var r = await RunUntyped(stateMonad, CancellationToken.None);

            if (r.IsSuccess)
                return Maybe<ISCLObject>.From(r.Value);
        }

        return Maybe<ISCLObject>.None;
    }

    /// <inheritdoc />
    public virtual bool ShouldBracketWhenSerialized => true;

    /// <inheritdoc />
    public Type OutputType => typeof(T);

    /// <summary>
    /// All properties of this step
    /// </summary>
    public IEnumerable<StepProperty> AllProperties
    {
        get
        {
            var r = GetType()
                .GetProperties()
                .Select(
                    propertyInfo => (propertyInfo,
                                     attribute: propertyInfo
                                         .GetCustomAttribute<StepPropertyBaseAttribute>())
                )
                .Where(x => x.attribute != null)
                .OrderByDescending(x => x.attribute?.Order != null)
                .ThenBy(x => x.attribute!.Order)
                .SelectMany((x, i) => GetMember(x.propertyInfo, x.attribute!, i).ToEnumerable());

            return r;

            Maybe<StepProperty> GetMember(
                PropertyInfo propertyInfo,
                StepPropertyBaseAttribute attribute,
                int index)
            {
                var val = propertyInfo.GetValue(this);

                var logAttribute = propertyInfo.GetCustomAttribute<LogAttribute>();

                var requiredVersions = propertyInfo.GetCustomAttributes<RequirementAttribute>()
                    .ToImmutableList();

                var stepParameter = new StepParameter(propertyInfo, attribute);

                if (val is IStep step)
                {
                    return new StepProperty.SingleStepProperty(
                        step,
                        stepParameter,
                        index,
                        logAttribute,
                        requiredVersions
                    );
                }

                if (val is LambdaFunction lf)
                {
                    return new StepProperty.LambdaFunctionProperty(
                        lf,
                        stepParameter,
                        index,
                        logAttribute,
                        requiredVersions
                    );
                }

                if (val is VariableName vn)
                {
                    return new StepProperty.VariableNameProperty(
                        vn,
                        stepParameter,
                        index,
                        logAttribute,
                        requiredVersions
                    );
                }

                if (val is IEnumerable<IStep> enumerable)
                {
                    var list = enumerable.ToList();

                    return new StepProperty.StepListProperty(
                        list,
                        stepParameter,
                        index,
                        logAttribute,
                        requiredVersions
                    );
                }

                return Maybe<StepProperty>.None;
            }
        }
    }

    /// <summary>
    /// Check that this step meets requirements
    /// </summary>
    public virtual Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore) =>
        Unit.Default;

    /// <inheritdoc />
    public Result<Unit, IError> Verify(StepFactoryStore stepFactoryStore)
    {
        var r0 = new[] { VerifyThis(stepFactoryStore) };

        var rRequirements = StepFactory.Requirements.Concat(RuntimeRequirements)
            .Select(req => req.Check(stepFactoryStore).MapError(x => x.WithLocation(this)));

        var r3 = AllProperties
            .Select(
                x => x switch
                {
                    StepProperty.SingleStepProperty singleStepProperty => singleStepProperty.Step
                        .Verify(stepFactoryStore),
                    StepProperty.StepListProperty stepListProperty => stepListProperty.StepList
                        .Select(s => s.Verify(stepFactoryStore))
                        .Combine(ErrorList.Combine)
                        .Map(_ => Unit.Default),
                    StepProperty.LambdaFunctionProperty lambda => lambda.LambdaFunction.Step.Verify(
                        stepFactoryStore
                    ),

                    StepProperty.VariableNameProperty _ => Unit.Default,
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                }
            );

        var finalResult = r0.Concat(rRequirements)
            .Concat(r3)
            .Combine(ErrorList.Combine)
            .Map(_ => Unit.Default);

        return finalResult;
    }

    /// <inheritdoc />
    public IEnumerable<(IStep Step, IStepParameter Parameter, IStep Value)> GetParameterValues()
    {
        foreach (var stepProperty in AllProperties)
        {
            switch (stepProperty)
            {
                case StepProperty.SingleStepProperty ssp:
                {
                    yield return (this, ssp.StepParameter, ssp.Step);

                    foreach (var nestedStep in ssp.Step.GetParameterValues())
                        yield return nestedStep;

                    break;
                }
                case StepProperty.StepListProperty slp:
                {
                    foreach (var listStep in slp.StepList)
                    {
                        foreach (var nestedStep in listStep.GetParameterValues())
                            yield return nestedStep;
                    }

                    break;
                }
                case StepProperty.LambdaFunctionProperty lfp:
                {
                    foreach (var lambdaStep in lfp.LambdaFunction.Step.GetParameterValues())
                    {
                        yield return lambdaStep;
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// If this step has a constant value. Convert it to a constant
    /// </summary>
    public IStep FoldIfConstant(
        StepFactoryStore sfs,
        IReadOnlyDictionary<VariableName, InjectedVariable> injectedVariables)
    {
        var constantValueTask = TryGetConstantValueAsync(
            ImmutableDictionary<VariableName, ISCLObject>.Empty,
            sfs
        );

        if (!constantValueTask.IsCompleted)
            return this;

        var constantValue = constantValueTask
            .Result.Bind(x => x.MaybeAs<T>());

        if (constantValue.HasNoValue)
            return this;

        return new SCLConstant<T>(constantValue.Value);
    }
}
