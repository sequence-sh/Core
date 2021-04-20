using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
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
    /// <summary>
    /// Create a new StepFactory
    /// </summary>
    protected StepFactory()
    {
        ScopedFunctionParameterReferences = new Lazy<IReadOnlySet<StepParameterReference>>(
            () => StepType.GetProperties()
                .Where(x => x.GetCustomAttribute<ScopedFunctionAttribute>() != null)
                .SelectMany(StepParameterReference.GetPossibleReferences)
                .ToHashSet()
        );
    }

    /// <inheritdoc />
    public abstract Result<TypeReference, IError> TryGetOutputTypeReference(
        TypeReference expectedTypeReference,
        FreezableStepData freezableStepData,
        TypeResolver typeResolver);

    /// <inheritdoc />
    public virtual IEnumerable<(VariableName variableName, TypeReference type)> GetVariablesSet(
        TypeReference expectedTypeReference,
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
                    .Select(GetUnderlyingType)
                    .Where(x => x.IsEnum)
                    .Concat(ExtraEnumTypes);

            static Type GetUnderlyingType(Type t)
            {
                while (true)
                {
                    if (!t.GenericTypeArguments.Any())
                        return t;

                    t = t.GenericTypeArguments.First();
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
        TypeReference expectedTypeReference,
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
        TypeReference expectedTypeReference,
        TypeResolver typeResolver,
        FreezableStepData freezeData)
    {
        var instanceResult = TryCreateInstance(expectedTypeReference, freezeData, typeResolver);

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

        List<(IFreezableStep, PropertyInfo)> scopedFunctions = new();

        foreach (var (stepMember, propertyInfo) in pairs)
        {
            if (propertyInfo.GetCustomAttribute<ScopedFunctionAttribute>() != null)
            {
                scopedFunctions.Add((stepMember.ConvertToStep(), propertyInfo));
                continue;
            }

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
                    propertyInfo,
                    step,
                    sList.List,
                    typeResolver
                ),
                _ => throw new ArgumentException("Step member wrong type"),
            };

            if (result.IsFailure)
                errors.Add(result.Error);
        }

        if (scopedFunctions.Any())
        {
            foreach (var (freezableStep, propertyInfo) in scopedFunctions)
            {
                var scopedContext = step.TryGetScopedTypeResolver(typeResolver, freezableStep);

                if (scopedContext.IsFailure)
                    errors.Add(scopedContext.Error);
                else
                {
                    remainingRequired.Remove(propertyInfo.Name);
                    var result = TrySetStep(propertyInfo, step, freezableStep, scopedContext.Value);

                    if (result.IsFailure)
                        errors.Add(result.Error);
                }
            }
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
        TextLocation? stepMemberLocation,
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

    private static Result<Unit, IError> TrySetStep(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        IFreezableStep freezableStep,
        TypeResolver typeResolver)
    {
        var freezeResult = freezableStep.TryFreeze(
            TypeReference.CreateFromStepType(propertyInfo.PropertyType),
            typeResolver
        );

        if (freezeResult.IsFailure)
            return freezeResult.ConvertFailure<Unit>();

        var stepToSet = TryCoerceStep(propertyInfo, freezeResult.Value);

        if (stepToSet.IsFailure)
            return stepToSet.MapError(x => x.WithLocation(parentStep)).ConvertFailure<Unit>();

        propertyInfo.SetValue(
            parentStep,
            stepToSet.Value
        ); //This could throw an exception but we don't expect it.

        return Unit.Default;
    }

    private static Result<IStep, IErrorBuilder> TryCoerceStep(
        PropertyInfo propertyInfo,
        IStep stepToSet)
    {
        if (propertyInfo.PropertyType.IsInstanceOfType(stepToSet))
            return Result.Success<IStep, IErrorBuilder>(stepToSet); //No coercion required

        if (propertyInfo.PropertyType.IsGenericType && stepToSet is StringConstant stringConstant)
        {
            if (propertyInfo.PropertyType.GenericTypeArguments.First().IsEnum)
            {
                var enumType = propertyInfo.PropertyType.GenericTypeArguments.First();

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
            else if (propertyInfo.PropertyType.GenericTypeArguments.First() == typeof(DateTime))
            {
                if (DateTime.TryParse(stringConstant.Value.GetString(), out var dt))
                {
                    var step = new DateTimeConstant(dt);
                    return Result.Success<IStep, IErrorBuilder>(step);
                }
            }
        }
        else if (propertyInfo.PropertyType.IsGenericType
              && propertyInfo.PropertyType.GenericTypeArguments.First() == typeof(object))
        {
            return Result.Success<IStep, IErrorBuilder>(stepToSet);
        }

        return ErrorCode.InvalidCast.ToErrorBuilder(propertyInfo.Name, stepToSet.Name);
    }

    private static Result<Unit, IError> TrySetStepList(
        PropertyInfo propertyInfo,
        ICompoundStep parentStep,
        IReadOnlyList<IFreezableStep> freezableStepList,
        TypeResolver typeResolver)
    {
        if (propertyInfo.GetCustomAttribute<StepListPropertyAttribute>() != null)
            return SetStepList();

        if (propertyInfo.GetCustomAttribute<StepPropertyAttribute>() != null)
        {
            var argument = propertyInfo.PropertyType.GenericTypeArguments.Single();

            if (argument.GenericTypeArguments.Length == 1
             && typeof(IArray).IsAssignableFrom(argument))
                return SetArray(argument);

            return Result.Failure<Unit, IError>(
                ErrorCode.WrongParameterType.ToErrorBuilder(
                        propertyInfo.Name,
                        argument.Name,
                        "Array/Sequence"
                    )
                    .WithLocation(parentStep)
            );
        }

        return Result.Failure<Unit, IError>(
            ErrorCode.WrongParameterType.ToErrorBuilder(
                    propertyInfo.Name,
                    MemberType.VariableName,
                    MemberType.StepList
                )
                .WithLocation(parentStep)
        );

        Result<Unit, IError> SetStepList()
        {
            var stepType = propertyInfo.PropertyType.GenericTypeArguments.Single();

            TypeReference expectedElementType = TypeReference.CreateFromStepType(stepType);

            var listType = typeof(List<>).MakeGenericType(stepType);

            var list      = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod(nameof(List<object>.Add))!;
            var errors    = new List<IError>();

            for (var index = 0; index < freezableStepList.Count; index++)
            {
                var freezableStep = freezableStepList[index];

                var freezeResult = freezableStep.TryFreeze(
                    expectedElementType,
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

            foreach (var freezableStep in freezableStepList)
            {
                var freezeResult = freezableStep.TryFreeze(
                    TypeReference.Create(nestedArgument),
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

            object array = CreateArray(list as dynamic);

            propertyInfo.SetValue(parentStep, array);

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

    private Lazy<IReadOnlySet<StepParameterReference>> ScopedFunctionParameterReferences
    {
        get;
    }

    /// <inheritdoc />
    public bool IsScopedFunction(StepParameterReference stepParameterReference) =>
        ScopedFunctionParameterReferences.Value.Contains(stepParameterReference);
}

}
