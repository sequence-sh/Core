namespace Reductech.Sequence.Core.Internal;

/// <summary>
/// A property of a step
/// </summary>
public abstract record StepProperty
(
    string Name,
    int Index,
    LogAttribute? LogAttribute,
    ImmutableList<RequirementAttribute> RequiredVersions)
{
    /// <summary>
    /// A lambda function
    /// </summary>
    public record LambdaFunctionProperty
    (
        LambdaFunction LambdaFunction,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        protected override string SerializeValue(SerializeOptions options)
        {
            return LambdaFunction.Serialize(options);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{Name} = {SerializeValue(SerializeOptions.Serialize)}";

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized => true;
    }

    /// <summary>
    /// A variable name
    /// </summary>
    public record VariableNameProperty(
        VariableName VariableName,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string ToString() => $"{Name} = {VariableName}";

        /// <inheritdoc />
        protected override string SerializeValue(SerializeOptions options) =>
            VariableName.Serialize(options);

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } = false;
    }

    /// <summary>
    /// A single step
    /// </summary>
    public record SingleStepProperty(
        IStep Step,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string ToString() => $"{Name} = {Step}";

        /// <inheritdoc />
        protected override string SerializeValue(SerializeOptions options) =>
            Step.Serialize(options);

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } =
            Step.ShouldBracketWhenSerialized;
    }

    /// <summary>
    /// A property which is a list of steps
    /// </summary>
    public record StepListProperty(
        IReadOnlyList<IStep> StepList,
        string Name,
        int Index,
        LogAttribute? LogAttribute,
        ImmutableList<RequirementAttribute> RequiredVersions) : StepProperty(
        Name,
        Index,
        LogAttribute,
        RequiredVersions
    )
    {
        /// <inheritdoc />
        public override string ToString() => $"{Name} = {StepList}";

        /// <inheritdoc />
        protected override string SerializeValue(SerializeOptions options)
        {
            var l = StepList.Select(
                    s =>
                    {
                        var ser = s.Serialize(options);

                        if (s.ShouldBracketWhenSerialized)
                        {
                            return $"({ser})";
                        }

                        return ser;
                    }
                )
                .ToList();

            return SerializationMethods.SerializeList(l);
        }

        /// <inheritdoc />
        protected override bool ShouldBracketWhenSerialized { get; } = false;
    }

    /// <summary>
    /// Get the serialized value
    /// </summary>
    protected abstract string SerializeValue(SerializeOptions options);

    /// <summary>
    /// Whether the value should be bracketed when it was serialized
    /// </summary>
    protected abstract bool ShouldBracketWhenSerialized { get; }

    /// <summary>
    /// Serialize this
    /// </summary>
    public string Serialize(SerializeOptions options)
    {
        var v = SerializeValue(options);

        if (ShouldBracketWhenSerialized)
            return $"({v})";

        return v;
    }
}
