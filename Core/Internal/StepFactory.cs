using System.Text.RegularExpressions;
using Namotion.Reflection;
using Sequence.Core.Steps;

namespace Sequence.Core.Internal;

/// <summary>
/// A factory for creating steps.
/// </summary>
public abstract class StepFactory : IStepFactory
{
    /// <inheritdoc />
    public abstract Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver);

    /// <inheritdoc />
    public virtual IEnumerable<UsedVariable> GetVariablesUsed(
        CallerMetadata callerMetadata,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver)
    {
        yield break;
    }

    /// <inheritdoc />
    public string TypeName => FormatTypeName(StepType);

    /// <summary>
    /// The type of this step.
    /// </summary>
    public abstract Type StepType { get; }

    /// <inheritdoc />
    public override string ToString() => TypeName;

    /// <inheritdoc />
    public IEnumerable<Type> EnumTypes
    {
        get
        {
            return
                StepType.GetProperties()
                    .Where(
                        property => property.GetCustomAttribute<StepPropertyBaseAttribute>() != null
                    )
                    .Select(x => x.PropertyType)
                    .SelectMany(GetUnderlyingTypes)
                    .Where(x => x.IsEnum)
                    .Concat(ExtraEnumTypes);

            static IEnumerable<Type> GetUnderlyingTypes(Type t)
            {
                if (!t.GenericTypeArguments.Any())
                    yield return t;
                else
                {
                    foreach (var arg in t.GenericTypeArguments)
                    {
                        foreach (var underlyingType in GetUnderlyingTypes(arg))
                        {
                            yield return underlyingType;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Additional enum types needed by this step.
    /// </summary>
    public virtual IEnumerable<Type> ExtraEnumTypes => Enumerable.Empty<Type>();

    /// <inheritdoc />
    public abstract string OutputTypeExplanation { get; }

    /// <inheritdoc />
    public virtual IStepSerializer Serializer => new FunctionSerializer(TypeName);

    /// <inheritdoc />
    public virtual IEnumerable<Requirement> Requirements => ImmutableArray<Requirement>.Empty;

    /// <summary>
    /// Creates an instance of this type.
    /// </summary>
    protected abstract Result<ICompoundStep, IError> TryCreateInstance(
        CallerMetadata callerMetadata,
        FreezableStepData freezeData,
        TypeResolver typeResolver);

    private IReadOnlyDictionary<StepParameterReference, IStepParameter>? _parameterDictionary;

    /// <inheritdoc />
    public IReadOnlyDictionary<StepParameterReference, IStepParameter> ParameterDictionary
    {
        get
        {
            return _parameterDictionary ??= StepType
                .GetProperties()
                .SelectMany(
                    propertyInfo => StepParameterReference.GetPossibleReferences(propertyInfo)
                        .Select(key => (key, parameter: StepParameter.TryCreate(propertyInfo)))
                )
                .Where(x => x.parameter is not null)
                .GroupBy(x => x.key, x => x.parameter!)
                .ToDictionary(
                    x => x.Key,
                    x => x.First()
                );
        }
    }

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver,
        FreezableStepData freezeData,
        OptimizationSettings settings)
    {
        var instanceResult = TryCreateInstance(callerMetadata, freezeData, typeResolver);

        if (instanceResult.IsFailure)
            return instanceResult.ConvertFailure<IStep>();

        var step = instanceResult.Value;
        step.TextLocation = freezeData.Location;

        var errors = new List<IError>();

        var pairs =
            new List<(FreezableStepProperty freezableStepProperty, PropertyInfo propertyInfo)>();

        var propertyDictionary = instanceResult.Value.GetType()
            .GetProperties()
            .SelectMany(
                propertyInfo => StepParameterReference.GetPossibleReferences(propertyInfo)
                    .Select(key => (propertyInfo, key))
            )
            .ToDictionary(x => x.key, x => x.propertyInfo);

        foreach (var (key, stepMember) in freezeData.StepProperties)
        {
            if (propertyDictionary.TryGetValue(key, out var propertyInfo))
                pairs.Add((stepMember, propertyInfo));
            else
                errors.Add(
                    ErrorCode.UnexpectedParameter.ToErrorBuilder(key.Name, TypeName)
                        .WithLocation(new ErrorLocation(TypeName, freezeData.Location))
                );
        }

        var duplicates = pairs
            .GroupBy(x => x.propertyInfo)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key);

        foreach (var propertyInfo in duplicates)
            errors.Add(
                ErrorCode.DuplicateParameter.ToErrorBuilder(propertyInfo.Name)
                    .WithLocation(freezeData)
            );

        if (errors.Any())
            return Result.Failure<IStep, IError>(ErrorList.Combine(errors));

        var remainingRequired =
            ParameterDictionary.Values.Where(x => x.Required)
                .Select(x => x.Name)
                .ToHashSet();

        foreach (var (stepMember, propertyInfo) in pairs)
        {
            remainingRequired.Remove(propertyInfo.Name);

            var result = stepMember switch
            {
                FreezableStepProperty.Variable vn => TrySetVariableName(
                    propertyInfo,
                    step,
                    vn.VName,
                    stepMember.Location,
                    typeResolver,
                    settings
                ),
                FreezableStepProperty.Step memberStep => TrySetStep(
                    propertyInfo,
                    step,
                    memberStep.FreezableStep,
                    typeResolver,
                    settings
                ),
                FreezableStepProperty.StepList sList => TrySetStepList(
                    propertyInfo,
                    step,
                    sList,
                    typeResolver,
                    settings
                ),
                FreezableStepProperty.Lambda lambda => TrySetLambda(
                    propertyInfo,
                    step,
                    lambda,
                    typeResolver,
                    settings
                ),

                _ => throw new ArgumentException("Step member wrong type"),
            };

            if (result.IsFailure)
                errors.Add(result.Error);
        }

        foreach (var property in remainingRequired)
            errors.Add(ErrorCode.MissingParameter.ToErrorBuilder(property).WithLocation(step));

        if (errors.Any())
            return Result.Failure<IStep, IError>(ErrorList.Combine(errors));

        return Result.Success<IStep, IError>(step);
    }

    /// <inheritdoc />
    public UnitResult<IError> CheckFreezePossible(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver,
        FreezableStepData freezeData)
    {
        var errors = new List<IError>();

        var pairs =
            new List<(FreezableStepProperty freezableStepProperty, IStepParameter stepParameter)>();

        foreach (var (key, stepMember) in freezeData.StepProperties)
        {
            if (ParameterDictionary.TryGetValue(key, out var stepParameter))
                pairs.Add((stepMember, stepParameter));
            else
                errors.Add(
                    ErrorCode.UnexpectedParameter.ToErrorBuilder(key.Name, TypeName)
                        .WithLocation(new ErrorLocation(TypeName, freezeData.Location))
                );
        }

        var duplicates = pairs
            .GroupBy(x => x.stepParameter)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key);

        foreach (var propertyInfo in duplicates)
            errors.Add(
                ErrorCode.DuplicateParameter.ToErrorBuilder(propertyInfo.Name)
                    .WithLocation(freezeData)
            );

        if (errors.Any())
            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

        var remainingRequired =
            ParameterDictionary.Values.Where(x => x.Required)
                .ToHashSet();

        foreach (var (stepMember, stepParameter) in pairs)
        {
            remainingRequired.Remove(stepParameter);
            var result = CheckStepMember(TypeName, stepMember, stepParameter, typeResolver);

            if (result.IsFailure)
                errors.Add(result.Error);
        }

        foreach (var property in remainingRequired)
            errors.Add(
                ErrorCode.MissingParameter.ToErrorBuilder(property).WithLocation(freezeData)
            );

        if (errors.Any())
            return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

        return UnitResult.Success<IError>();
    }

    private static UnitResult<IError> CheckStepMember(
        string stepTypeName,
        FreezableStepProperty stepMember,
        IStepParameter stepParameter,
        TypeResolver typeResolver)
    {
        switch (stepParameter.MemberType)
        {
            case MemberType.VariableName:
            {
                return stepMember.AsVariableName(stepParameter.Name).Map(_ => Unit.Default);
            }
            case MemberType.Step:
            {
                var stepCallerMetadata = new CallerMetadata(
                    stepTypeName,
                    stepParameter.Name,
                    stepParameter.StepTypeReference
                );

                return stepMember.ConvertToStep()
                    .CheckFreezePossible(stepCallerMetadata, typeResolver);
            }
            case MemberType.Lambda:
            {
                var typeToSet = stepParameter.StepType;

                if (!typeToSet.IsGenericType || typeToSet.GenericTypeArguments.Length != 2)
                    throw new Exception($"{stepTypeName}.{stepParameter.Name} is not a lambda");

                var inputTypeArgument  = typeToSet.GenericTypeArguments[0];
                var inputTypeReference = TypeReference.Create(inputTypeArgument);

                var outputTypeArgument  = typeToSet.GenericTypeArguments[1];
                var outputTypeReference = TypeReference.Create(outputTypeArgument);

                if (inputTypeReference.IsUnknown || outputTypeReference.IsUnknown)
                    return
                        UnitResult
                            .Success<IError>(); //We can't prove that freeze is impossible because types are unknown

                var lambda = stepMember.ConvertToLambda();

                var stepCallerMetadata = new CallerMetadata(
                    stepTypeName,
                    stepParameter.Name,
                    outputTypeReference
                );

                var scopedTypeResolver = typeResolver.TryCloneWithScopedLambda(
                    lambda,
                    inputTypeReference,
                    stepCallerMetadata
                );

                if (scopedTypeResolver.IsFailure)
                    return scopedTypeResolver.ConvertFailure<Unit>();

                var result = lambda.FreezableStep.CheckFreezePossible(
                    stepCallerMetadata,
                    scopedTypeResolver.Value
                );

                return result;
            }

            case MemberType.StepList:
            {
                var stepListResult = stepMember.AsStepList(stepParameter.Name);

                if (stepListResult.IsFailure)
                    return stepListResult.ConvertFailure<Unit>();

                if (!stepParameter.StepType.IsGenericType
                 || stepParameter.StepType.GenericTypeArguments.Length != 1)
                    throw new Exception($"{stepTypeName}.{stepParameter.Name} is not a list");

                var elementType =
                    TypeReference.Create(stepParameter.StepType.GenericTypeArguments[0]);

                var nestedErrors = new List<IError>();

                for (var index = 0; index < stepListResult.Value.Count; index++)
                {
                    var nestedCallerMetadata = new CallerMetadata(
                        stepTypeName,
                        stepParameter.Name + $"[{index}]",
                        elementType
                    );

                    var freezableStep = stepListResult.Value[index];

                    var freezeResult = freezableStep.CheckFreezePossible(
                        nestedCallerMetadata,
                        typeResolver
                    );

                    if (freezeResult.IsFailure)
                        nestedErrors.Add(freezeResult.Error);
                }

                if (nestedErrors.Any())
                    return Result.Failure<Unit, IError>(ErrorList.Combine(nestedErrors));

                return UnitResult.Success<IError>();
            }

            default: throw new ArgumentException("Step member wrong type");
        }
    }

    private static Result<Unit, IError> TrySetVariableName(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        VariableName variableName,
        TextLocation stepMemberLocation,
        TypeResolver typeResolver,
        OptimizationSettings optimizationSettings)
    {
        if (propertyInfo.PropertyType.IsInstanceOfType(variableName))
        {
            propertyInfo.SetValue(parentStep, variableName);
            return Unit.Default;
        }

        var step = FreezableFactory.CreateFreezableGetVariable(variableName, stepMemberLocation);

        return TrySetStep(propertyInfo, parentStep, step, typeResolver, optimizationSettings);
    }

    private static Result<Unit, IError> TrySetLambda(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        FreezableStepProperty.Lambda lambda,
        TypeResolver typeResolver,
        OptimizationSettings optimizationSettings)
    {
        var typeToSet = propertyInfo.PropertyType;

        if (!typeToSet.IsGenericType)
            return ErrorCode.InvalidCast
                .ToErrorBuilder(
                    $"{parentStep.StepFactory.TypeName}.{propertyInfo.Name}",
                    nameof(MemberType.Lambda)
                )
                .WithLocationSingle(lambda.Location);

        var inputTypeArgument  = typeToSet.GenericTypeArguments[0];
        var inputTypeReference = TypeReference.Create(inputTypeArgument);

        var outputTypeArgument  = typeToSet.GenericTypeArguments[1];
        var outputTypeReference = TypeReference.Create(outputTypeArgument);

        var callerMetadata = new CallerMetadata(
            parentStep.Name,
            propertyInfo.Name,
            outputTypeReference
        );

        var typeResolverResult = typeResolver.TryCloneWithScopedLambda(
            lambda,
            inputTypeReference,
            callerMetadata
        );

        if (typeResolverResult.IsFailure)
            return typeResolverResult.ConvertFailure<Unit>();

        var frozenStep = lambda.FreezableStep.TryFreeze(
            callerMetadata,
            typeResolverResult.Value,
            optimizationSettings
        );

        if (frozenStep.IsFailure)
            return frozenStep.ConvertFailure<Unit>();

        try
        {
            var lambdaInstance = Activator.CreateInstance(
                typeToSet,
                lambda.VName,
                frozenStep.Value
            );

            propertyInfo.SetValue(parentStep, lambdaInstance);
        }
        catch (Exception e) //I'm nervous this might happen
        {
            var error = ErrorCode.Unknown.ToErrorBuilder(e)
                .WithLocationSingle(lambda.Location);

            return error;
        }

        return Unit.Default;
    }

    private static Result<Unit, IError> TrySetStep(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        IFreezableStep freezableStep,
        TypeResolver typeResolver,
        OptimizationSettings optimizationSettings)
    {
        if (propertyInfo.GetCustomAttribute<FunctionPropertyAttribute>() is not null)
        {
            return TrySetLambda(
                propertyInfo,
                parentStep,
                new FreezableStepProperty.Lambda(null, freezableStep, freezableStep.TextLocation),
                typeResolver,
                optimizationSettings
            );
        }

        var expectedType = TypeReference.CreateFromParameterProperty(propertyInfo);

        var callerMetadata = new CallerMetadata(
            parentStep.Name,
            propertyInfo.Name,
            expectedType
        );

        var freezeResult = freezableStep.TryFreeze(
            callerMetadata,
            typeResolver,
            optimizationSettings
        );

        if (freezeResult.IsFailure)
            return freezeResult.ConvertFailure<Unit>();

        var stepToSet = freezeResult.Value.TryCoerce(
            propertyInfo.Name,
            propertyInfo.PropertyType
        );

        if (stepToSet.IsFailure)
            return stepToSet.MapError(x => x.WithLocation(parentStep)).ConvertFailure<Unit>();

        propertyInfo.SetValue(
            parentStep,
            stepToSet.Value
        ); //This could throw an exception but we don't expect it.

        return Unit.Default;
    }

    private static Result<Unit, IError> TrySetStepList(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        FreezableStepProperty.StepList stepList,
        TypeResolver typeResolver,
        OptimizationSettings optimizationSettings)
    {
        if (propertyInfo.GetCustomAttribute<StepListPropertyAttribute>() != null)
            return SetStepList();

        if (propertyInfo.GetCustomAttribute<StepPropertyAttribute>() != null)
        {
            var stepType = propertyInfo.PropertyType.GenericTypeArguments.Single();

            if (stepType.GenericTypeArguments.Length == 1
             && typeof(IArray).IsAssignableFrom(stepType))
                return SetArray(stepType);

            if (stepType.GetInterfaces().Contains(typeof(ISCLOneOf)))
            {
                foreach (var optionType in stepType.GenericTypeArguments)
                {
                    if (typeof(IArray).IsAssignableFrom(optionType)
                     && optionType.GenericTypeArguments.Length == 1)
                    {
                        var r = SetArray(optionType);

                        if (r.IsSuccess)
                            return r;
                    }
                }
            }

            return ErrorCode.WrongType.ToErrorBuilder(
                    parentStep.Name,
                    TypeReference.CreateFromStepType(stepType).Name,
                    propertyInfo.Name,
                    stepList.ConvertToStep().StepName,
                    "Array/Sequence"
                )
                .WithLocationSingle(new ErrorLocation(parentStep.Name, parentStep.TextLocation));
        }

        return ErrorCode.WrongType.ToErrorBuilder(
                parentStep.Name,
                nameof(VariableName),
                propertyInfo.Name,
                stepList.ConvertToStep().StepName,
                "Array/Sequence"
            )
            .WithLocationSingle(new ErrorLocation(parentStep.Name, parentStep.TextLocation));

        Result<Unit, IError> SetStepList()
        {
            var stepType = propertyInfo.PropertyType.GenericTypeArguments.Single();

            var expectedElementType = TypeReference.CreateFromStepType(stepType);

            var listType = typeof(List<>).MakeGenericType(stepType);

            var list      = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
            var errors    = new List<IError>();

            for (var index = 0; index < stepList.List.Count; index++)
            {
                var nestedCallerMetadata = new CallerMetadata(
                    parentStep.Name,
                    propertyInfo.Name + $"[{index}]",
                    expectedElementType
                );

                var freezableStep = stepList.List[index];

                var freezeResult = freezableStep.TryFreeze(
                    nestedCallerMetadata,
                    typeResolver,
                    optimizationSettings
                );

                if (freezeResult.IsFailure)
                    errors.Add(freezeResult.Error);
                else if (freezeResult.Value is IConstantStep constant
                      && stepType.IsInstanceOfType(constant.Value))
                {
                    addMethod.Invoke(list, new object?[] { constant.Value });
                }
                else if (stepType.IsInstanceOfType(freezeResult.Value))
                {
                    addMethod.Invoke(list, new object?[] { freezeResult.Value });
                }
                else
                {
                    var error = ErrorCode.InvalidCast.ToErrorBuilder(
                            propertyInfo.Name + $"[{index}]",
                            CompressSpaces(freezeResult.Value.Name)
                        )
                        .WithLocation(parentStep);

                    errors.Add(error);
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            propertyInfo.SetValue(parentStep, list);

            return Unit.Default;
        }

        Result<Unit, IError> SetArray(Type argument)
        {
            var nestedArgument = argument.GenericTypeArguments.Single();

            var stepType = typeof(IStep<>).MakeGenericType(nestedArgument);

            var listType  = typeof(List<>).MakeGenericType(stepType);
            var addMethod = listType.GetMethod(nameof(List<object>.Add))!;

            var list   = Activator.CreateInstance(listType);
            var errors = new List<IError>();

            var nestedCallerMetadata = new CallerMetadata(
                parentStep.Name,
                propertyInfo.Name,
                TypeReference.Create(nestedArgument)
            );

            for (var index = 0; index < stepList.List.Count; index++)
            {
                var freezableStep = stepList.List[index];

                var freezeResult = freezableStep.TryFreeze(
                    nestedCallerMetadata,
                    typeResolver,
                    optimizationSettings
                );

                if (freezeResult.IsFailure)
                    errors.Add(freezeResult.Error);
                else if (stepType.IsInstanceOfType(freezeResult.Value))
                {
                    addMethod.Invoke(list, new object?[] { freezeResult.Value });
                }
                else
                {
                    var propertyName =
                        $"{parentStep.StepFactory.TypeName}.{propertyInfo.Name}[{index}]";

                    var coercedValue = freezeResult.Value.TryCoerce(propertyName, stepType);

                    if (coercedValue.IsSuccess)
                        addMethod.Invoke(list, new object?[] { coercedValue.Value });
                    else
                        errors.Add(coercedValue.Error.WithLocation(parentStep));
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            IStep stepToSet = CreateArray(list as dynamic);

            if (propertyInfo.PropertyType.GenericTypeArguments[0]
                .GetInterfaces()
                .Contains(typeof(ISCLOneOf)))
            {
                stepToSet = OneOfStep.Create(
                    propertyInfo.PropertyType.GenericTypeArguments[0],
                    stepToSet
                );
            }

            try
            {
                propertyInfo.SetValue(parentStep, stepToSet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return Unit.Default;
        }
    }

    private static ArrayNew<T> CreateArray<T>(List<IStep<T>> list) where T : ISCLObject
    {
        var step = ArrayNew<T>.CreateArray(list);
        return step;
    }

    private static readonly Regex SpaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static string CompressSpaces(string stepName) => SpaceRegex.Replace(stepName, " ");

    /// <summary>
    /// Creates a typed generic step with one type argument.
    /// </summary>
    protected static Result<ICompoundStep, IErrorBuilder> TryCreateGeneric(
        Type openGenericType,
        Type parameterType)
    {
        object? r;

        try
        {
            var genericType = openGenericType.MakeGenericType(parameterType);
            r = Activator.CreateInstance(genericType);
        }
        catch (ArgumentException e)
        {
            if (e.Message.Contains("violates the constraint of type") && e.Message.Contains(
                    "comparable",
                    StringComparison.OrdinalIgnoreCase
                ))
            {
                var parameterTypeName = parameterType.GetDisplayName();
                return new ErrorBuilder(ErrorCode.TypeNotComparable, parameterTypeName);
            }

            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }

        catch (Exception e)
        {
            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }

        if (r is ICompoundStep rp)
            return Result.Success<ICompoundStep, IErrorBuilder>(rp);

        return new ErrorBuilder(
            ErrorCode.CannotCreateGeneric,
            openGenericType.Name.Split("`")[0],
            parameterType.GetDisplayName()
        );
    }

    /// <summary>
    /// Creates a typed generic step with multiple type arguments.
    /// </summary>
    protected static Result<ICompoundStep, IErrorBuilder> TryCreateGeneric(
        Type openGenericType,
        Type[] parameterTypes)
    {
        object? r;

        try
        {
            var genericType = openGenericType.MakeGenericType(parameterTypes);
            r = Activator.CreateInstance(genericType);
        }
        catch (ArgumentException e)
        {
            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }
        catch (Exception e)
        {
            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }

        if (r is ICompoundStep rp)
            return Result.Success<ICompoundStep, IErrorBuilder>(rp);

        return new ErrorBuilder(
            ErrorCode.CannotCreateGeneric,
            openGenericType.Name.Split("`")[0],
            string.Join(",", parameterTypes.Select(x => x.GetDisplayName()))
        );
    }

    /// <summary>
    /// Gets the name of the type, removing the backtick if it is a generic type.
    /// </summary>
    protected static string FormatTypeName(Type type)
    {
        string friendlyName = type.Name;

        if (type.IsGenericType)
        {
            var iBacktick = friendlyName.IndexOf('`');

            if (iBacktick > 0)
                friendlyName = friendlyName.Remove(iBacktick);
        }

        return friendlyName;
    }

    /// <inheritdoc />
    public virtual string Category
    {
        get
        {
            var fullName = StepType.Assembly.GetName().Name!;
            var lastTerm = GetLastTerm(fullName);

            return lastTerm;
        }
    }

    private static string GetLastTerm(string s) =>
        s.Split('.', StringSplitOptions.RemoveEmptyEntries).Last();

    /// <inheritdoc />
    public string Summary => StepType.GetXmlDocsSummary();

    /// <inheritdoc />
    public IEnumerable<string> Names
    {
        get
        {
            yield return TypeName;

            foreach (var alias in StepType.GetCustomAttributes<AliasAttribute>())
                yield return alias.Name;
        }
    }

    /// <inheritdoc />
    public IEnumerable<SCLExample> Examples => StepType.GetCustomAttributes<SCLExampleAttribute>()
        .Where(x => x.IncludeInDocumentation)
        .Select(x => x.ToSCLExample);
}
