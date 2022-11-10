namespace Sequence.Core.Steps;

/// <summary>
/// Map each element of an array or entity stream to a new value.
/// This Step can be used to update elements of an array, or to update
/// entity property values.
/// The new value must have the same type as the original value.
/// </summary>
[SCLExample("ArrayMap [1, 2, 3, 4] Function: (<> + 1)", "[2, 3, 4, 5]")]
[SCLExample("ArrayMap [1,2,3] ($\"Num {<>}\")", "[\"Num 1\", \"Num 2\", \"Num 3\"]")]
[SCLExample("ArrayMap [(a: 1), (a: 2), (a: 3), (a: 4)] Function: (<>['a'] + 1)", "[2, 3, 4, 5]")]
[SCLExample(
    @"Map Array: [
  ('type': 'A', 'value': 1)
  ('type': 'B', 'value': 2)
  ('type': 'A', 'value': 3)
] Using: (In <> Set: 'value' To: (<>['value'] + 1))",
    "[('type': \"A\" 'value': 2), ('type': \"B\" 'value': 3), ('type': \"A\" 'value': 4)]"
)]
[Alias("EntityMap")] // legacy name
[Alias("Map")]
[AllowConstantFolding]
public sealed class ArrayMap<TIn, TOut> : CompoundStep<Array<TOut>>
    where TIn : ISCLObject
    where TOut : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Array<TOut>, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        var arrayResult = await Array.Run(stateMonad, cancellationToken);

        if (arrayResult.IsFailure)
            return arrayResult.ConvertFailure<Array<TOut>>();

        var currentState = stateMonad.GetState().ToImmutableDictionary();

        async ValueTask<TOut> Action(TIn record)
        {
            await using var scopedMonad = new ScopedStateMonad(
                stateMonad,
                currentState,
                Function.VariableNameOrItem,
                new KeyValuePair<VariableName, ISCLObject>(Function.VariableNameOrItem, record)
            );

            var result = await Function.StepTyped.Run(scopedMonad, cancellationToken);

            if (result.IsFailure)
                throw new ErrorException(result.Error);

            return result.Value;
        }

        var newStream = arrayResult.Value.SelectAwait(Action);

        return newStream;
    }

    /// <summary>
    /// The array or entity stream to map
    /// </summary>
    [StepProperty(1)]
    [Required]
    public IStep<Array<TIn>> Array { get; set; } = null!;

    /// <summary>
    /// A function to update the values and return the mapped entity
    /// </summary>
    [FunctionProperty(2)]
    [Required]
    [Alias("Using")]
    public LambdaFunction<TIn, TOut> Function { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => ArrayMapStepFactory.Instance;

    /// <summary>
    /// Filter entities according to a function.
    /// </summary>
    private sealed class ArrayMapStepFactory : StepFactory
    {
        private ArrayMapStepFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static StepFactory Instance { get; } = new ArrayMapStepFactory();

        private string ArrayPropertyName => nameof(ArrayMap<ISCLObject, ISCLObject>.Array);

        /// <inheritdoc />
        public override Result<TypeReference, IError> TryGetOutputTypeReference(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var r = TryGetOutputMemberType(callerMetadata, freezableStepData, typeResolver);

            if (r.IsFailure)
                return r.ConvertFailure<TypeReference>();

            return new TypeReference.Array(r.Value);
        }

        private Result<TypeReference, IError> TryGetOutputMemberType(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var lambda = freezableStepData.TryGetLambda(LambdaPropertyName, StepType);

            if (lambda.IsFailure)
                return lambda.ConvertFailure<TypeReference>();

            var expectedType =
                callerMetadata.ExpectedType.TryGetArrayMemberTypeReference(typeResolver)
                    .MapError(x => x.WithLocation(freezableStepData));

            //Try to get the output type from the input type and the lambda

            var inputMemberType = TryGetInputMemberType(
                callerMetadata,
                freezableStepData,
                typeResolver
            );

            if (inputMemberType.IsFailure)
                return expectedType;

            var scopedCallerMetadata = new CallerMetadata(
                StepType.Name,
                LambdaPropertyName,
                TypeReference.Unknown.Instance
            );

            var scopedTypeResolver =
                typeResolver.TryCloneWithScopedLambda(
                    lambda.Value,
                    inputMemberType.Value,
                    scopedCallerMetadata
                );

            if (scopedTypeResolver.IsFailure)
                return expectedType;

            var outputMemberType =
                lambda.Value.FreezableStep.TryGetOutputTypeReference(
                    scopedCallerMetadata,
                    scopedTypeResolver.Value
                );

            if (outputMemberType.IsFailure)
                return expectedType;

            if (expectedType.IsFailure)
                return expectedType;

            return outputMemberType.Value.TryCombine(expectedType.Value, typeResolver)
                .MapError(x => x.WithLocation(freezableStepData));
        }

        private Result<TypeReference, IError> TryGetInputMemberType(
            CallerMetadata _,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var arrayStep = freezableStepData.TryGetStep(ArrayPropertyName, StepType);

            if (arrayStep.IsFailure)
                return arrayStep.ConvertFailure<TypeReference>();

            TypeReference expectedType = new TypeReference.Array(TypeReference.Unknown.Instance);

            var cm = new CallerMetadata(StepType.Name, ArrayPropertyName, expectedType);

            var result =
                arrayStep.Value.TryGetOutputTypeReference(cm, typeResolver)
                    .Bind(
                        x => x.TryGetArrayMemberTypeReference(typeResolver)
                            .MapError(error => error.WithLocation(freezableStepData))
                    );

            return result;
        }

        /// <inheritdoc />
        public override Type StepType => typeof(ArrayMap<,>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "Array of T";

        /// <inheritdoc />
        protected override Result<ICompoundStep, IError> TryCreateInstance(
            CallerMetadata callerMetadata,
            FreezableStepData freezeData,
            TypeResolver typeResolver)
        {
            var inputType = TryGetInputMemberType(callerMetadata, freezeData, typeResolver)
                .Bind(x => x.TryGetType(typeResolver).MapError(e => e.WithLocation(freezeData)));

            if (inputType.IsFailure)
                return inputType.ConvertFailure<ICompoundStep>();

            var outputType = TryGetOutputMemberType(callerMetadata, freezeData, typeResolver)
                .Bind(x => x.TryGetType(typeResolver).MapError(e => e.WithLocation(freezeData)));

            if (outputType.IsFailure)
                return outputType.ConvertFailure<ICompoundStep>();

            var typeList = new[] { inputType.Value, outputType.Value };

            var result = TryCreateGeneric(StepType, typeList)
                .MapError(e => e.WithLocation(freezeData));

            return result;
        }

        private string LambdaPropertyName => nameof(ArrayMap<ISCLObject, ISCLObject>.Function);
    }
}
