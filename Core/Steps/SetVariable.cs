namespace Sequence.Core.Steps;

/// <summary>
/// Sets the value of a named variable.
/// </summary>
public sealed class SetVariable<T> : CompoundStep<Unit> where T : ISCLObject
{
    /// <inheritdoc />
    protected override async ValueTask<Result<Unit, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        return await Value.Run(stateMonad, cancellationToken)
            .Bind(x => stateMonad.SetVariableAsync(Variable, x, true, this, cancellationToken));
    }

    /// <inheritdoc />
    public override IStepFactory StepFactory => SetVariableStepFactory.Instance;

    /// <summary>
    /// The name of the variable to set.
    /// </summary>
    [VariableName(1)]
    [Required]
    public VariableName Variable { get; set; }

    /// <summary>
    /// The value to set the variable to.
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<T> Value { get; set; } = null!;

    /// <summary>
    /// Sets the value of a named variable.
    /// </summary>
    private class SetVariableStepFactory : GenericStepFactory
    {
        private SetVariableStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static GenericStepFactory Instance { get; } = new SetVariableStepFactory();

        /// <inheritdoc />
        public override Type StepType => typeof(SetVariable<>);

        /// <inheritdoc />
        public override string OutputTypeExplanation => nameof(Unit);

        /// <inheritdoc />
        public override TypeReference
            GetOutputTypeReference(TypeReference memberTypeReference) =>
            TypeReference.Unit.Instance;

        /// <inheritdoc />
        protected override Result<TypeReference, IError> GetGenericTypeParameter(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var valueType = GetValueType(freezableStepData, typeResolver);

            if (valueType.IsFailure)
                return valueType.ConvertFailure<TypeReference>();

            var vn = freezableStepData
                .TryGetVariableName(nameof(SetVariable<ISCLObject>.Variable), StepType)
                .Map(x => new TypeReference.Variable(x));

            if (vn.IsFailure)
                return vn.ConvertFailure<TypeReference>();

            var combinedType = valueType.Value.TryCombine(vn.Value, typeResolver);

            return combinedType.MapError(x => x.WithLocation(freezableStepData));
        }

        private Result<TypeReference, IError> GetValueType(
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var r =
                freezableStepData
                    .TryGetStep(nameof(SetVariable<ISCLObject>.Value), StepType)
                    .Bind(
                        x =>
                            x.TryGetOutputTypeReference(
                                new CallerMetadata(
                                    TypeName,
                                    nameof(SetVariable<ISCLObject>.Value),
                                    TypeReference.Any.Instance
                                ),
                                typeResolver
                            )
                    );

            return r;
        }

        /// <inheritdoc />
        public override IEnumerable<UsedVariable> GetVariablesUsed(
            CallerMetadata callerMetadata,
            FreezableStepData freezableStepData,
            TypeResolver typeResolver)
        {
            var vn = freezableStepData.TryGetVariableName(
                nameof(SetVariable<ISCLObject>.Variable),
                StepType
            );

            if (vn.IsFailure)
                yield break;

            var memberType = GetValueType(
                freezableStepData,
                typeResolver
            );

            if (memberType.IsFailure)
                yield return new(
                    vn.Value,
                    TypeReference.Unknown.Instance,
                    true,
                    freezableStepData
                        .Location
                );
            else
                yield return new(vn.Value, memberType.Value, true, freezableStepData.Location);
        }

        /// <inheritdoc />
        public override IStepSerializer Serializer => SetVariableStepSerializer.SerializerInstance;

        private class SetVariableStepSerializer : IStepSerializer
        {
            private SetVariableStepSerializer() { }

            /// <summary>
            /// The instance
            /// </summary>
            public static IStepSerializer SerializerInstance { get; } =
                new SetVariableStepSerializer();

            /// <inheritdoc />
            public string Serialize(
                SerializeOptions options,
                IEnumerable<StepProperty> stepProperties)
            {
                var list     = stepProperties.ToList();
                var variable = list.OfType<StepProperty.VariableNameProperty>().Single();
                var step     = list.OfType<StepProperty.SingleStepProperty>().Single();

                var vString = variable.Serialize(options);
                var sString = step.Serialize(options);

                var r = $"{vString} = {sString}";

                return r;
            }

            /// <inheritdoc />
            public void Format(
                IEnumerable<StepProperty> stepProperties,
                TextLocation? textLocation,
                IndentationStringBuilder indentationStringBuilder,
                FormattingOptions options,
                Stack<Comment> remainingComments)
            {
                var list     = stepProperties.ToList();
                var variable = list.OfType<StepProperty.VariableNameProperty>().Single();
                var step     = list.OfType<StepProperty.SingleStepProperty>().Single();

                variable.Format(indentationStringBuilder, options, remainingComments);
                indentationStringBuilder.Append(" = ");
                step.Format(indentationStringBuilder, options, remainingComments);
            }
        }
    }
}
