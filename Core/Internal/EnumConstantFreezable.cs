namespace Reductech.EDR.Core.Internal;

/// <summary>
/// A freezable enum constant.
/// This will fail at freezing if the EnumType is not available in the Step Factory Store
/// </summary>
public sealed record EnumConstantFreezable(
    string EnumType,
    string EnumValue,
    TextLocation TextLocation) : IFreezableStep
{
    /// <inheritdoc />
    public bool Equals(IFreezableStep? other)
    {
        return other is EnumConstantFreezable o && EnumType == o.EnumType
                                                && EnumValue == o.EnumValue;
    }

    /// <inheritdoc />
    public string StepName => $"{EnumType}.{EnumValue}";

    /// <inheritdoc />
    public Result<IStep, IError> TryFreeze(CallerMetadata callerMetadata, TypeResolver typeResolver)
    {
        var convertResult = TryConvert(typeResolver.StepFactoryStore);

        if (convertResult.IsFailure)
            return convertResult.ConvertFailure<IStep>();

        var stepType = typeof(SCLConstant<>).MakeGenericType(convertResult.Value.GetType());

        var stepInstance = (IStep)Activator.CreateInstance(stepType, convertResult.Value)!;

        return Result.Success<IStep, IError>(stepInstance);
    }

    /// <inheritdoc />
    public Result<IReadOnlyCollection<UsedVariable>, IError> GetVariablesUsed(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver)
    {
        return Result.Success<IReadOnlyCollection<UsedVariable>, IError>(
            ImmutableList<UsedVariable>.Empty
        );
    }

    /// <inheritdoc />
    public Result<TypeReference, IError> TryGetOutputTypeReference(
        CallerMetadata callerMetadata,
        TypeResolver typeResolver) =>
        TryConvert(typeResolver.StepFactoryStore).Map(x => x.GetTypeReference());

    private Result<ISCLEnum, IError> TryConvert(StepFactoryStore stepFactoryStore)
    {
        if (!stepFactoryStore.EnumTypesDictionary.TryGetValue(EnumType, out var type)
         || !type.IsEnum)
            return ErrorCode.UnexpectedEnumType.ToErrorBuilder(EnumType)
                .WithLocationSingle(TextLocation);

        if (!Enum.TryParse(type, EnumValue, true, out var value))
            return ErrorCode.UnexpectedEnumValue.ToErrorBuilder(EnumType, EnumValue)
                .WithLocationSingle(TextLocation);

        var sclEnumType = typeof(SCLEnum<>).MakeGenericType(type);

        var sclInstance = (ISCLEnum)Activator.CreateInstance(sclEnumType, value)!;

        return Result.Success<ISCLEnum, IError>(sclInstance);
    }

    /// <inheritdoc />
    public IFreezableStep ReorganizeNamedArguments(StepFactoryStore stepFactoryStore) => this;
}
