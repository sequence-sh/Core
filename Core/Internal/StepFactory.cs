using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using OneOf;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Core.Internal
{

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

    private IReadOnlyDictionary<StepParameterReference, PropertyInfo>? _propertyDictionary;

    /// <inheritdoc />
    public IReadOnlyDictionary<StepParameterReference, PropertyInfo> ParameterDictionary
    {
        get
        {
            return _propertyDictionary ??= StepType
                .GetProperties()
                .SelectMany(
                    propertyInfo => StepParameterReference.GetPossibleReferences(propertyInfo)
                        .Select(key => (propertyInfo, key))
                )
                .ToDictionary(x => x.key, x => x.propertyInfo);
        }
    }

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver,
        FreezableStepData freezeData)
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
            ParameterDictionary.Values.Where(x => x.GetCustomAttributes<RequiredAttribute>().Any())
                .Select(x => x.Name)
                .ToHashSet();

        foreach (var (stepMember, propertyInfo) in pairs)
        {
            var nestedCallerMetadata =
                new CallerMetadata(
                    TypeName,
                    propertyInfo.Name,
                    TypeReference.Create(
                        propertyInfo.PropertyType.GenericTypeArguments.FirstOrDefault()
                     ?? typeof(object)
                    )
                );

            remainingRequired.Remove(propertyInfo.Name);

            var result = stepMember switch
            {
                FreezableStepProperty.Variable vn => TrySetVariableName(
                    propertyInfo,
                    step,
                    vn.VName,
                    stepMember.Location,
                    typeResolver
                ),
                FreezableStepProperty.Step memberStep => TrySetStep(
                    propertyInfo,
                    step,
                    memberStep.FreezableStep,
                    typeResolver
                ),
                FreezableStepProperty.StepList sList => TrySetStepList(
                    nestedCallerMetadata,
                    propertyInfo,
                    step,
                    sList,
                    typeResolver
                ),
                FreezableStepProperty.Lambda lambda => TrySetLambda(
                    propertyInfo,
                    step,
                    lambda,
                    typeResolver
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

    private static Result<Unit, IError> TrySetVariableName(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        VariableName variableName,
        TextLocation stepMemberLocation,
        TypeResolver typeResolver)
    {
        if (propertyInfo.PropertyType.IsInstanceOfType(variableName))
        {
            propertyInfo.SetValue(parentStep, variableName);
            return Unit.Default;
        }

        var step = FreezableFactory.CreateFreezableGetVariable(variableName, stepMemberLocation);

        return TrySetStep(propertyInfo, parentStep, step, typeResolver);
    }

    private static Result<Unit, IError> TrySetLambda(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        FreezableStepProperty.Lambda lambda,
        TypeResolver typeResolver)
    {
        var typeToSet          = propertyInfo.PropertyType;
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

        var frozenStep = lambda.FreezableStep.TryFreeze(callerMetadata, typeResolverResult.Value);

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
        TypeResolver typeResolver)
    {
        if (propertyInfo.GetCustomAttribute<FunctionPropertyAttribute>() is not null)
        {
            return TrySetLambda(
                propertyInfo,
                parentStep,
                new FreezableStepProperty.Lambda(null, freezableStep, freezableStep.TextLocation),
                typeResolver
            );
        }

        var freezeResult = freezableStep.TryFreeze(
            new CallerMetadata(
                parentStep.Name,
                propertyInfo.Name,
                TypeReference.CreateFromStepType(propertyInfo.PropertyType)
            ),
            typeResolver
        );

        if (freezeResult.IsFailure)
            return freezeResult.ConvertFailure<Unit>();

        var stepToSet = TryCoerceStep(
            propertyInfo.Name,
            propertyInfo.PropertyType,
            freezeResult.Value
        );

        if (stepToSet.IsFailure)
            return stepToSet.MapError(x => x.WithLocation(parentStep)).ConvertFailure<Unit>();

        propertyInfo.SetValue(
            parentStep,
            stepToSet.Value
        ); //This could throw an exception but we don't expect it.

        return Unit.Default;
    }

    private static Result<IStep, IErrorBuilder> TryCoerceStep(
        string propertyName,
        Type propertyType,
        IStep stepToSet)
    {
        if (propertyType.IsInstanceOfType(stepToSet))
            return Result.Success<IStep, IErrorBuilder>(stepToSet); //No coercion required

        if (propertyType.IsGenericType)
        {
            var nestedType = propertyType.GenericTypeArguments.First();

            if (nestedType.GetInterfaces().Contains(typeof(IOneOf)))
            {
                var oneOfTypes = nestedType.GenericTypeArguments;

                foreach (var oneOfType in oneOfTypes)
                {
                    var stepType     = typeof(IStep<>).MakeGenericType(oneOfType);
                    var coerceResult = TryCoerceStep(propertyName, stepType, stepToSet);

                    if (coerceResult.IsSuccess)
                    {
                        var resultStep = OneOfStep.Create(nestedType, stepToSet);
                        return Result.Success<IStep, IErrorBuilder>(resultStep);
                    }
                }
            }
            else if (stepToSet is StringConstant stringConstant)
            {
                if (nestedType.IsEnum)
                {
                    var enumType = propertyType.GenericTypeArguments.First();

                    if (Enum.TryParse(
                        enumType,
                        stringConstant.Value.GetString(),
                        true,
                        out var enumValue
                    ))
                    {
                        var step = EnumConstantFreezable.TryCreateEnumConstant(enumValue!);
                        return step;
                    }
                }
                else if (nestedType == typeof(DateTime))
                {
                    if (DateTime.TryParse(stringConstant.Value.GetString(), out var dt))
                    {
                        var step = new DateTimeConstant(dt);
                        return Result.Success<IStep, IErrorBuilder>(step);
                    }
                }
            }
            else if (nestedType == typeof(object))
            {
                return Result.Success<IStep, IErrorBuilder>(stepToSet);
            }
        }

        return ErrorCode.InvalidCast.ToErrorBuilder(propertyName, stepToSet.Name);
    }

    private static Result<Unit, IError> TrySetStepList(
        CallerMetadata callerMetadata,
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        FreezableStepProperty.StepList stepList,
        TypeResolver typeResolver)
    {
        if (propertyInfo.GetCustomAttribute<StepListPropertyAttribute>() != null)
            return SetStepList();

        if (propertyInfo.GetCustomAttribute<StepPropertyAttribute>() != null)
        {
            var stepType = propertyInfo.PropertyType.GenericTypeArguments.Single();

            if (stepType.GenericTypeArguments.Length == 1
             && typeof(IArray).IsAssignableFrom(stepType))
                return SetArray(stepType);

            if (stepType.GetInterfaces().Contains(typeof(IOneOf)))
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

            return callerMetadata.GetWrongTypeError(
                stepList.ConvertToStep().StepName,
                "Array/Sequence",
                new ErrorLocation(parentStep.Name, parentStep.TextLocation)
            );
        }

        return callerMetadata.GetWrongTypeError(
            stepList.ConvertToStep().StepName,
            nameof(VariableName),
            new ErrorLocation(parentStep.Name, parentStep.TextLocation)
        );

        Result<Unit, IError> SetStepList()
        {
            var stepType = propertyInfo.PropertyType.GenericTypeArguments.Single();

            TypeReference expectedElementType = TypeReference.CreateFromStepType(stepType);

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
                    typeResolver
                );

                if (freezeResult.IsFailure)
                    errors.Add(freezeResult.Error);
                else if (freezeResult.Value is IConstantStep constant
                      && stepType.IsInstanceOfType(constant.ValueObject))
                {
                    addMethod.Invoke(list, new[] { constant.ValueObject });
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

            foreach (var freezableStep in stepList.List)
            {
                var freezeResult = freezableStep.TryFreeze(
                    nestedCallerMetadata,
                    typeResolver
                );

                if (freezeResult.IsFailure)
                    errors.Add(freezeResult.Error);
                else if (stepType.IsInstanceOfType(freezeResult.Value))
                {
                    addMethod.Invoke(list, new object?[] { freezeResult.Value });
                }
                else
                {
                    var error = ErrorCode.InvalidCast.ToErrorBuilder(
                            parentStep.StepFactory.TypeName,
                            CompressSpaces(freezeResult.Value.Name)
                        )
                        .WithLocation(parentStep);

                    errors.Add(error);
                }
            }

            if (errors.Any())
                return Result.Failure<Unit, IError>(ErrorList.Combine(errors));

            IStep stepToSet = CreateArray(list as dynamic);

            if (propertyInfo.PropertyType.GenericTypeArguments[0]
                .GetInterfaces()
                .Contains(typeof(IOneOf)))
            {
                stepToSet = OneOfStep.Create(
                    propertyInfo.PropertyType.GenericTypeArguments[0],
                    stepToSet
                );
            }

            propertyInfo.SetValue(parentStep, stepToSet);

            return Unit.Default;
        }
    }

    private static ArrayNew<T> CreateArray<T>(List<IStep<T>> list)
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
            if (e.Message.Contains("violates the constraint of type"))
            {
                var parameterTypeName = parameterType.GetDisplayName();
                return new ErrorBuilder(ErrorCode.TypeNotComparable, parameterTypeName);
            }

            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }
        #pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception e)
        {
            return ErrorCode.InvalidCast.ToErrorBuilder(e);
        }
        #pragma warning restore CA1031 // Do not catch general exception types

        if (r is ICompoundStep rp)
            return Result.Success<ICompoundStep, IErrorBuilder>(rp);

        return new ErrorBuilder(
            ErrorCode.CannotCreateGeneric,
            openGenericType.Name.Split("`")[0],
            parameterType.GetDisplayName()
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
    public IEnumerable<SCLExampleAttribute> Examples =>
        StepType.GetCustomAttributes<SCLExampleAttribute>();
}

}
