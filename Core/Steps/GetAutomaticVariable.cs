namespace Reductech.Sequence.Core.Steps;

/// <summary>
/// Gets the value of the automatic variable
/// </summary>
[AllowConstantFolding]
public sealed class GetAutomaticVariable<T> : CompoundStep<T> where T : ISCLObject
{
    /// <inheritdoc />
    #pragma warning disable 1998
    protected override async ValueTask<Result<T, IError>> Run(
        #pragma warning restore 1998
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        if (stateMonad.AutomaticVariable.HasNoValue)
            return Result.Failure<T, IError>(
                ErrorCode.AutomaticVariableNotSet.ToErrorBuilder().WithLocation(this)
            );

        var result = stateMonad.GetVariable<T>(stateMonad.AutomaticVariable.GetValueOrThrow())
            .MapError(x => x.WithLocation(this));

        return result;
    }

    /// <inheritdoc />
    public override bool ShouldBracketWhenSerialized => false;

    /// <inheritdoc />
    public override IStepFactory StepFactory => GetAutomaticVariableStepFactory.Instance;

    private class GetAutomaticVariableStepFactory : GenericStepFactory
    {
        private GetAutomaticVariableStepFactory() { }
        public static GenericStepFactory Instance { get; } = new GetAutomaticVariableStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(GetAutomaticVariable<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => "T";

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) => memberTypeReference;

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            yield return new(
                VariableName.Item,
                callerMetadata.ExpectedType,
                false,
                freezableStepData
                    .Location
            );
        }

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var avr = typeResolver.AutomaticVariableName;

            if (avr.HasNoValue)
                return ErrorCode.AutomaticVariableNotSet.ToErrorBuilder()
                    .WithLocationSingle(
                        new ErrorLocation(
                            TypeName,
                            freezableStepData.Location
                        )
                    );

            var expectedTypeReference = callerMetadata.ExpectedType;

            if (typeResolver.Dictionary.TryGetValue(avr.GetValueOrThrow(), out var tr))
            {
                var result = tr.TypeReference.TryCombine(expectedTypeReference, typeResolver);
                return result.MapError(x => x.WithLocation(freezableStepData));
            }

            return TypeReference.AutomaticVariable.Instance;
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer =>
            GetAutomaticVariableStepSerializer.SerializerInstance;

        private class GetAutomaticVariableStepSerializer : IStepSerializer
        {
            private GetAutomaticVariableStepSerializer() { }

            /// <summary>
            /// The instance
            /// </summary>
            public static IStepSerializer SerializerInstance { get; } =
                new GetAutomaticVariableStepSerializer();

            /// <inheritdoc />
            public string Serialize(SerializeOptions _, IEnumerable<StepProperty> stepProperties)
            {
                return "<>";
            }

            /// <inheritdoc />
            public void Format(
                IEnumerable<StepProperty> stepProperties,
                TextLocation? textLocation,
                IndentationStringBuilder indentationStringBuilder,
                FormattingOptions options,
                Stack<Comment>? remainingComments = null)
            {
                indentationStringBuilder.AppendPrecedingComments(
                    remainingComments ?? new Stack<Comment>(),
                    textLocation
                );

                indentationStringBuilder.Append("<>");
            }
        }
    }
}
